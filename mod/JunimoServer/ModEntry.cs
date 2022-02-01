using JunimoServer.Services.GameCreator;
using JunimoServer.Services.GameLoader;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using SimpleHttp;
using System.Threading;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using JunimoServer.Controllers;
using System;
using HarmonyLib;
using JunimoServer.Services.AlwaysOnServer;
using JunimoServer.Services.ChatCommands;
using JunimoServer.Services.CropSaver;

namespace JunimoServer
{

    internal class ModEntry : Mod
    {

        private GameCreatorService gameCreatorService;
        private GameCreatorController gameCreatorController;
        private GameLoaderService gameLoaderService;
        private IModHelper helper;
        private bool titleLaunched = false;

        public override void Entry(IModHelper helper)
        {
            var harmony = new Harmony(this.ModManifest.UniqueID);


            var chatCommands = new ChatCommands(Monitor, harmony);
            var cropSaver = new CropSaver(helper, harmony, Monitor);
            var alwaysOnServer = new AlwaysOnServer(helper, Monitor);
            this.helper = helper;
            gameLoaderService = new GameLoaderService(helper, Monitor);
            gameCreatorService = new GameCreatorService(gameLoaderService);
            gameCreatorController = new GameCreatorController(Monitor, gameCreatorService);
            helper.Events.Display.RenderedActiveMenu += OnRenderedActiveMenu;
            helper.Events.Input.ButtonPressed += OnButtonPressed;

            Monitor.Log("REST API starting: http://localhost:8081", LogLevel.Info);

            var cts = new CancellationTokenSource();
            HttpServer.ListenAsync(8081, cts.Token, OnHttp);

            Monitor.Log("REST API has started: http://localhost:8081", LogLevel.Info);
            
     



            //GRPCGameManagerService gameManagerService = new GRPCGameManagerService(gameCreator, gameLoader, Monitor);
            ////ReflectionServiceImpl reflectionServiceImpl = new ReflectionServiceImpl(GameManagerService.Descriptor);
            //Server server = new Server
            //{
            //    Services = {
            //        GameManagerService.BindService(gameManagerService)
            //        //ServerReflection.BindService(reflectionServiceImpl),
            //    },
            //    Ports = { new ServerPort("0.0.0.0", 50051, ServerCredentials.Insecure) }
            //};
            //this.Monitor.Log($"Starting Server on 50051...", LogLevel.Debug);

            //server.Start();


        }

        private async Task OnHttp(HttpListenerRequest rq, HttpListenerResponse res)
        {
            try
            {
                Dictionary<string, string> queryArgs = new Dictionary<string, string>();

                if (rq.HttpMethod == "POST" && rq.Url.PathAndQuery.TryMatch("/game", queryArgs))
                {

                    string body = rq.ParseBody();
                    if (body != null)
                    {
                        gameCreatorController.CreateGame(body);
                        res.AsText("Success!");
                    }
                    else
                    {
                        res.AsText("No body");
                    }
                }
                else if(rq.HttpMethod == "GET" && rq.Url.PathAndQuery.TryMatch("/startup", queryArgs))
                {
                    if (titleLaunched)
                    {
                        res.StatusCode = 200;
                        res.AsText("Ready!");
                    }
                    else
                    {
                        res.StatusCode = 500;
                        res.AsText("Not Ready!");
                    }
                }
                else
                {
                    res.StatusCode = 404;
                    res.AsText("Route not found");
                }
            }
            catch (Exception ex)
            {
                res.StatusCode = 500;
                res.AsText(ex.Message);
            }


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
            if (gameLoaderService.HasLoadableSave())
            {
                gameLoaderService.LoadSave();
            }
        }
    }

}
