using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HarmonyLib;
using JunimoServer.Services.AlwaysOnServer;
using JunimoServer.Services.Backup;
using JunimoServer.Services.ChatCommands;
using JunimoServer.Services.Commands;
using JunimoServer.Services.CropSaver;
using JunimoServer.Services.Daemon;
using JunimoServer.Services.GameCreator;
using JunimoServer.Services.GameLoader;
using JunimoServer.Services.GameTweaks;
using JunimoServer.Services.PersistentOption;
using JunimoServer.Services.ServerOptim;
using JunimoServer.Util;
using SimpleHttp;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace JunimoServer
{
    internal class ModEntry : Mod
    {
        private static readonly string DaemonPort = Environment.GetEnvironmentVariable("DAEMON_HTTP_PORT") ?? "8080";
        private const bool DisableRendering = true;

        private GameCreatorService _gameCreatorService;
        private GameLoaderService _gameLoaderService;
        private ServerOptimizer _serverOptimizer;
        private bool _titleLaunched = false;


        public override void Entry(IModHelper helper)
        {
            Game1.options.pauseWhenOutOfFocus = false;

            var harmony = new Harmony(this.ModManifest.UniqueID);
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri($"http://localhost:{DaemonPort}");

            var options = new PersistentOptions(helper);
            var chatCommands = new ChatCommands(Monitor, harmony, helper);
            var cropSaver = new CropSaver(helper, harmony, Monitor);
            _serverOptimizer = new ServerOptimizer(harmony, Monitor);
            var alwaysOnServer = new AlwaysOnServer(helper, Monitor, chatCommands, _serverOptimizer, DisableRendering);
            _gameLoaderService = new GameLoaderService(helper, Monitor);
            var daemonService = new DaemonService(httpClient, Monitor);
            _gameCreatorService = new GameCreatorService(_gameLoaderService, options, Monitor, daemonService);
            var backupService = new BackupService(httpClient, Monitor);
            var backupScheduler = new BackupScheduler(helper, backupService, Monitor);
            var gameTweaker = new GameTweaker(helper);

            CabinCommand.Register(helper, chatCommands, options, Monitor);

            helper.Events.Display.RenderedActiveMenu += OnRenderedActiveMenu;
            helper.Events.Input.ButtonPressed += OnButtonPressed;

            helper.Events.Multiplayer.PeerConnected += OnPeerConnected;

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

        private void OnPeerConnected(object sender, PeerConnectedEventArgs e)
        {
            ConditionallySendWelcomeMessage();
        }

        private bool _sentWelcomeMessage = false;

        private void ConditionallySendWelcomeMessage()
        {
            if (Helper.GetCurrentNumCabins() > 1 || _sentWelcomeMessage) return;
            Helper.SendPublicMessage("!cabin");
            Helper.SendPublicMessage("Type !cabin to build a cabin. This will allow more people to join.");
            
            Helper.SendPublicMessage("");
            
            Helper.SendPublicMessage("!help");
            Helper.SendPublicMessage("Type !help for more info.");
            _sentWelcomeMessage = true;
        }

        private async Task OnHttp(HttpListenerRequest rq, HttpListenerResponse res)
        {
            try
            {
                Dictionary<string, string> queryArgs = new Dictionary<string, string>();

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
                Monitor.Log($"{Game1.player.getTileLocation()}", LogLevel.Debug);
                Monitor.Log($"{Game1.player.team.farmhandsCanMoveBuildings.Value}", LogLevel.Debug);
                Monitor.Log($"{Game1.activeClickableMenu.GetType().Name}", LogLevel.Debug);
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
            else
            {
                _gameCreatorService.CreateNewGameFromDaemonConfig();
            }
        }
    }
}