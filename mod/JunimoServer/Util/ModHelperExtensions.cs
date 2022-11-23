using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.SDKs;

namespace JunimoServer.Util
{
    public static class ModHelperExtensions
    {
        public static Multiplayer GetMultiplayer(this IModHelper helper)
        {
            return helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();
        }

        public static void SendPublicMessage(this IModHelper helper, string msg)
        {
            helper.GetMultiplayer()
                .sendChatMessage(LocalizedContentManager.CurrentLanguageCode, msg, Multiplayer.AllPlayers);
        }

        public static void SendPrivateMessage(this IModHelper helper, long uniqueMultiplayerId, string msg)
        {
            helper.GetMultiplayer()
                .sendChatMessage(LocalizedContentManager.CurrentLanguageCode, msg, uniqueMultiplayerId);
        }

        public static int GetCurrentNumCabins(this IModHelper helper)
        {
            return helper.Reflection.GetMethod(Game1.server, "cabins")
                .Invoke<IEnumerable<Cabin>>().ToList().Count;
        }
        
        public static bool IsNetworkingReady(this IModHelper helper)
        {
            var sdk = helper.Reflection.GetField<SDKHelper>(typeof(Program), "_sdk").GetValue();
            return sdk != null && sdk.Networking != null;
        }

        public static long GetOwnerPlayerId(this IModHelper helper)
        {
           return ((Cabin)Game1.getFarm().buildings.First(building => building.isCabin).indoors.Value).owner.UniqueMultiplayerID;
        }
    }
}