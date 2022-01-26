using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace JunimoServer.Services.CropSaver
{
    public class CropSaver
    {

        private readonly CropWatcher _cropWatcher;
        private readonly CropSaverDataLoader _cropSaverDataLoader;

        private readonly IMonitor _monitor;
        private readonly IModHelper _helper;

        public CropSaver(IModHelper helper, Harmony harmony, IMonitor monitor)
        {
            _monitor = monitor;
            _helper = helper;
            _cropWatcher = new CropWatcher(helper, OnCropAdded, OnCropRemoved);
            _cropSaverDataLoader = new CropSaverDataLoader(helper);
            CropSaverOverrides.Initialize(helper, _monitor, _cropSaverDataLoader);
            harmony.Patch(
                original: AccessTools.Method(typeof(Crop), nameof(Crop.Kill)),
                prefix: new HarmonyMethod(typeof(CropSaverOverrides), nameof(CropSaverOverrides.KillCrop_Prefix))
            );

            helper.Events.GameLoop.Saving += OnSaving;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.DayEnding += OnDayEnd;
        }


        private void OnDayEnd(object sender, DayEndingEventArgs e)
        {
            //prolong crops
            var onlineIds = new HashSet<long>();
            foreach (var farmer in Game1.getOnlineFarmers())
            {
                onlineIds.Add(farmer.UniqueMultiplayerID);
            }

            _cropSaverDataLoader.GetSaverCrops().ForEach(saverCrop =>
            {
                var dirt = saverCrop.TryGetCoorespondingDirt();
                if (dirt != null)
                {
                    if (!onlineIds.Contains(saverCrop.ownerId) && dirt.needsWatering())
                    {
                        saverCrop.IncrementExtraDays();
                    }
                }
            });

            //remove crops
            for (var i = _cropSaverDataLoader.GetSaverCrops().Count - 1; i >= 0; i--)
            {
                var saverCrop = _cropSaverDataLoader.GetSaverCrops()[i];
                var crop = saverCrop.TryGetCoorespondingCrop();
                if (crop == null)
                {
                    _monitor.Log(
                        $"Crop at {saverCrop.cropLocationTile.X}, {saverCrop.cropLocationTile.Y} was still " +
                        $"being managed by CropSaver after death." +
                        $"\nRemoving from managed crops...", LogLevel.Warn);
                    _cropSaverDataLoader.RemoveCrop(saverCrop.cropLocationName, saverCrop.cropLocationTile);
                    continue;
                }


                var dateOfDeath = CalculateDateOfDeath(crop, saverCrop);
                var fullyGrown = CalculateFullyGrown(crop);
                
                _monitor.Log($"Crop owned by {saverCrop.ownerId} will die when sleeping on {dateOfDeath}");

                var isAfterDateOfDeath = SDate.Now() >= dateOfDeath;

                if (isAfterDateOfDeath && !(fullyGrown && onlineIds.Contains(saverCrop.ownerId)))
                {
                    _cropSaverDataLoader.RemoveCrop(saverCrop.cropLocationName, saverCrop.cropLocationTile);
                    var dead = _helper.Reflection.GetField<NetBool>(crop, "dead").GetValue();
                    var raisedSeeds = _helper.Reflection.GetField<NetBool>(crop, "raisedSeeds").GetValue();

                    dead.Value = true;
                    raisedSeeds.Value = false;
                    
                    _monitor.Log($"Killing crop owned by {saverCrop.ownerId}");
                }
            }
        }

        private bool CalculateFullyGrown(Crop crop)
        {
            var currentPhase = _helper.Reflection.GetField<NetInt>(crop, "currentPhase").GetValue().Value;
            var phaseDays = _helper.Reflection.GetField<NetIntList>(crop, "phaseDays").GetValue();


            var fullyGrown = (currentPhase >= phaseDays.Count - 1);
            return fullyGrown;
        }

        private static SDate CalculateDateOfDeath(Crop crop, SaverCrop saverCrop)
        {
            var numSeasons = crop.seasonsToGrowIn.Count -
                             (crop.seasonsToGrowIn.IndexOf(saverCrop.datePlanted.Season));
            var numDaysToLive = saverCrop.extraDays + (28 * numSeasons) - saverCrop.datePlanted.Day;
            var dateOfDeath = saverCrop.datePlanted.AddDays(numDaysToLive);
            
            return dateOfDeath;
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            _cropSaverDataLoader.LoadDataFromDisk();
        }

        private void OnSaving(object sender, SavingEventArgs e)
        {
            _cropSaverDataLoader.SaveDataToDisk();
        }


        private void OnCropAdded(TerrainFeature feature)
        {
            var closestFarmer = FarmerUtil.GetClosestFarmer(feature.currentLocation, feature.currentTileLocation);
            _cropSaverDataLoader.AddCrop(new SaverCrop(
                    feature.currentLocation.Name,
                    feature.currentTileLocation,
                    closestFarmer.UniqueMultiplayerID,
                    SDate.Now()
                )
            );

            _monitor.Log(
                $"Added crop planted at: {feature.currentLocation} on: {feature.currentTileLocation} by: {closestFarmer.Name}");
        }

        private void OnCropRemoved(TerrainFeature feature)
        {
            _cropSaverDataLoader.RemoveCrop(feature.currentLocation.Name, feature.currentTileLocation);
            _monitor.Log(
                $"Removed crop at: {feature.currentLocation} on: {feature.currentTileLocation}");
        }
    }
}