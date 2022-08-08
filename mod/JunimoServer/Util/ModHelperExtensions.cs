using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;

namespace JunimoServer.Util
{
    public static class ModHelperExtensions
    {
        public static void SendPublicMessage(this IModHelper helper, string msg)
        {
            var multiplayer = helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();
            multiplayer.sendChatMessage(LocalizedContentManager.CurrentLanguageCode, msg, Multiplayer.AllPlayers);
        }

        public static void SendPrivateMessage(this IModHelper helper, long uniqueMultiplayerId, string msg)
        {
            var multiplayer = helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();
            multiplayer.sendChatMessage(LocalizedContentManager.CurrentLanguageCode, msg, uniqueMultiplayerId);
        }

        public static int GetCurrentNumCabins(this IModHelper helper)
        {
            return helper.Reflection.GetMethod(Game1.server, "cabins")
                .Invoke<IEnumerable<Cabin>>().ToList().Count;
        }
    }
}