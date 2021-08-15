using JunimoServer.Services.GameCreator;
using JunimoServer.Services.GameLoader;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace JunimoServer
{
    internal class ModEntry : Mod
    {

        private GameCreatorService gameCreator;
        private GameLoaderService gameLoader;
        private IModHelper helper;
        private bool titleLaunched = false;
        public override void Entry(IModHelper helper)
        {
            this.helper = helper;
            gameCreator = new GameCreatorService();
            gameLoader = new GameLoaderService(helper, Monitor);
            helper.Events.Display.RenderedActiveMenu += OnRenderedActiveMenu;
            helper.Events.Input.ButtonPressed += OnButtonPressed;

        }

        private void OnButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (e.Button == SButton.Q)
            {
                this.Monitor.Log($"{Constants.SaveFolderName}", LogLevel.Debug);
            }
        }

        private void OnRenderedActiveMenu(object sender, StardewModdingAPI.Events.RenderedActiveMenuEventArgs e)
        {
            if (Game1.activeClickableMenu is TitleMenu && !titleLaunched)
            {
                OnTitleMenuLaunched();
                titleLaunched = true;
            }
        }

        private void OnTitleMenuLaunched()
        {
            if (gameLoader.HasLoadableSave())
            {
                gameLoader.LoadSave();
            }
            else
            {

                string farmName = "LoadTest3";
                gameCreator.CreateNewGame(new NewGameConfig { FarmName = farmName });

                // Save name goes from nothing -> _uuid -> farmName_uuid (once loaded in completely)
                // This should always result in the correct name
                gameLoader.SetSaveNameToLoad(farmName + Constants.SaveFolderName);
            }

        }
    }

}
