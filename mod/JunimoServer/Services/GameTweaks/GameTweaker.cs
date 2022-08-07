using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace JunimoServer.Services.GameTweaks
{
    public class GameTweaker
    {
        public GameTweaker(IModHelper helper)
        {
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            Game1.options.setMoveBuildingPermissions("on");
        }
    }
}