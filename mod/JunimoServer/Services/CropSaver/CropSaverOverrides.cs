using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace JunimoServer.Services.CropSaver
{
    public class CropSaverOverrides
    {
        private static IModHelper _helper;
        private static IMonitor _monitor;

        public static void Initialize(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;
        }
        
        public static bool KillCrop_Prefix(ref Crop __instance)
        {
            return false;
        }
        
        
        public static void Plant_Postfix(ref HoeDirt __instance, ref bool __result, int index, int tileX, int tileY, Farmer who, bool isFertilizer, GameLocation location)
        {
            if (__result && __instance.crop != null && location.Name.Equals("Farm"))
            {
                _monitor.Log($"{location.Name}, {new Vector2(tileX, tileY)}, {who.UniqueMultiplayerID}, {SDate.Now()}");   
                // SaverCrop crop = new SaverCrop(location.Name, new Vector2(tileX, tileY), who.UniqueMultiplayerID, SDate.Now());
                // Loader.ClientAddCrop(crop);
            }

        }
    }
}