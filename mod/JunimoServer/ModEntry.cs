using JunimoServer.Services.GameCreator;
using JunimoServer.Services.GameLoader;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using JunimoServer.GRPC;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using JunimoServer.Services.GRPCGameManager;
using Grpc.Reflection;
using System.Collections.Generic;
using Grpc.Reflection.V1Alpha;

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

            GRPCGameManagerService gameManagerService = new GRPCGameManagerService(gameCreator, gameLoader);
            ReflectionServiceImpl reflectionServiceImpl = new ReflectionServiceImpl(GameManagerService.Descriptor);
            Server server = new Server
            {
                Services = {
                    GameManagerService.BindService(gameManagerService),
                    ServerReflection.BindService(reflectionServiceImpl),
                },
                Ports = { new ServerPort("0.0.0.0", 50051, ServerCredentials.Insecure) }
            };
            server.Start();


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
        }
    }

}
