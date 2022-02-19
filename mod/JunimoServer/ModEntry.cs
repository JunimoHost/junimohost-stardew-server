using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using HarmonyLib;
using JunimoServer.Controllers;
using JunimoServer.Services.AlwaysOnServer;
using JunimoServer.Services.ChatCommands;
using JunimoServer.Services.CropSaver;
using JunimoServer.Services.GameCreator;
using JunimoServer.Services.GameLoader;
using JunimoServer.Services.ServerOptimizer;
using SimpleHttp;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace JunimoServer
{
    internal class ModEntry : Mod
    {
        private GameCreatorService _gameCreatorService;
        private GameCreatorController _gameCreatorController;
        private GameLoaderService _gameLoaderService;
        private ServerOptimizer _serverOptimizer;
        private bool _titleLaunched = false;

        public override void Entry(IModHelper helper)
        {
            var harmony = new Harmony(this.ModManifest.UniqueID);


            var chatCommands = new ChatCommands(Monitor, harmony);
            var cropSaver = new CropSaver(helper, harmony, Monitor);
            var alwaysOnServer = new AlwaysOnServer(helper, Monitor, chatCommands);
            _serverOptimizer = new ServerOptimizer(harmony, Monitor);
            _gameLoaderService = new GameLoaderService(helper, Monitor);
            _gameCreatorService = new GameCreatorService(_gameLoaderService);
            _gameCreatorController = new GameCreatorController(Monitor, _gameCreatorService);
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
                        res.StatusCode = 200;
                        res.AsText("Creating Game!");
                        _gameCreatorController.CreateGame(body);
                    }
                    else
                    {
                        res.AsText("No body");
                    }
                }

                if (rq.HttpMethod == "GET" && rq.Url.PathAndQuery.TryMatch("/game/status", queryArgs))
                {
                    if (_gameCreatorService.GameIsCreating)
                    {
                        res.StatusCode = 202;
                        res.AsText("Not Ready!");
                    }
                    else
                    {
                        res.StatusCode = 200;
                        res.AsText("Ready!");
                    }
                }
                else if (rq.HttpMethod == "GET" && rq.Url.PathAndQuery.TryMatch("/startup", queryArgs))
                {
                    if (_titleLaunched)
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

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (e.Button == SButton.Q)
            {
                this.Monitor.Log($"{Constants.SaveFolderName}", LogLevel.Debug);
            }
        }

        private void OnRenderedActiveMenu(object sender, RenderedActiveMenuEventArgs e)
        {
            if (Game1.activeClickableMenu is TitleMenu && !_titleLaunched)
            {
                OnTitleMenuLaunched();
                _titleLaunched = true;
            }
        }

        private void OnTitleMenuLaunched()
        {
            if (_gameLoaderService.HasLoadableSave())
            {
                _gameLoaderService.LoadSave();
            }

            // _serverOptimizer.DisableDrawing();
        }
    }
}