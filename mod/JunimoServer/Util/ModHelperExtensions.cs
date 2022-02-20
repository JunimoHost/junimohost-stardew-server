using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

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
    }
}