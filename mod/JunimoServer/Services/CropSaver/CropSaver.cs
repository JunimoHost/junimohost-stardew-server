using System.Linq;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace JunimoServer.Services.CropSaver
{
    public class CropSaver
    {
        private const string CropSaverSaveKey = "JunimoHost.CropSaver";

        private readonly CropWatcher _cropWatcher;

        private IMonitor _monitor;

        public CropSaver(IModHelper helper, Harmony harmony, IMonitor monitor)
        {
            _monitor = monitor;
            _cropWatcher = new CropWatcher(helper, OnCropAdded);
            CropSaverOverrides.Initialize(helper,_monitor);
            harmony.Patch(
                original: AccessTools.Method(typeof(Crop), nameof(Crop.Kill)),
                prefix: new HarmonyMethod(typeof(CropSaverOverrides), nameof(CropSaverOverrides.KillCrop_Prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(HoeDirt), nameof(HoeDirt.plant)),
                postfix: new HarmonyMethod(typeof(CropSaverOverrides), nameof(CropSaverOverrides.Plant_Postfix))
            );
            _monitor.Log("Patching CropSaver Methods");
            
        }

        private void OnCropAdded(TerrainFeature feature)
        {
            var farmers = Game1.getOnlineFarmers();
            var closestFarmer = Game1.player; // assign ownership of crop to host as fallback (should only be the case if crop planting was automated)
            var closestDistance = float.MaxValue;
            foreach (var farmer in farmers)
            {
                if (!farmer.currentLocation.Equals(feature.currentLocation)) continue;
                var farmerDistance = Vector2.Distance(farmer.getTileLocation(), feature.currentTileLocation);
                if (farmerDistance < closestDistance)
                {
                    closestFarmer = farmer;
                    closestDistance = farmerDistance;
                }
            }
            
            _monitor.Log($"Crop plated at: {feature.currentLocation} on: {feature.currentTileLocation} by: {closestFarmer.Name}");


        }



    }
}