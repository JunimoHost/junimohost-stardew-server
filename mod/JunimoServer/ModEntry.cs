using JunimoServer.Services.GameCreator;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace JunimoServer
{
    internal class ModEntry : Mod
    {

        private GameCreatorService gameCreator;
        private IModHelper helper;
        private bool created = false;
        public override void Entry(IModHelper helper)
        {
            this.helper = helper;
            gameCreator = new GameCreatorService();
            helper.Events.Display.RenderedActiveMenu += OnRenderedActiveMenu;
            
        }

        private void OnRenderedActiveMenu(object sender, StardewModdingAPI.Events.RenderedActiveMenuEventArgs e)
        {
            if (Game1.activeClickableMenu is TitleMenu && !created)
            {
                Monitor.Log("onrendered on menu", LogLevel.Debug);
                gameCreator.CreateNewGame(new NewGameConfig { WhichFarm = 2 });
                created = true;


            }
        }


    }

}
