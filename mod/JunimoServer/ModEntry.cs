using System;
using System.Net.Http;
using HarmonyLib;
using JunimoServer.Services.AlwaysOnServer;
using JunimoServer.Services.Backup;
using JunimoServer.Services.CabinManager;
using JunimoServer.Services.ChatCommands;
using JunimoServer.Services.Commands;
using JunimoServer.Services.CropSaver;
using JunimoServer.Services.Daemon;
using JunimoServer.Services.GalaxyAuth;
using JunimoServer.Services.GameCreator;
using JunimoServer.Services.GameLoader;
using JunimoServer.Services.GameTweaks;
using JunimoServer.Services.HostAutomation;
using JunimoServer.Services.NetworkTweaks;
using JunimoServer.Services.PersistentOption;
using JunimoServer.Services.ServerOptim;
using JunimoServer.Services.SteamAuth;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace JunimoServer
{
    internal class ModEntry : Mod
    {
        private static readonly string SteamAuthServerAddress =
            Environment.GetEnvironmentVariable("STEAM_AUTH_IP_PORT") ?? "localhost:8083";

        private static readonly string DaemonPort = Environment.GetEnvironmentVariable("DAEMON_HTTP_PORT") ?? "8080";

        private static readonly bool DisableRendering =
            bool.Parse(Environment.GetEnvironmentVariable("DISABLE_RENDERING") ?? "true");

        private static readonly bool ForceNewDebugGame =
            bool.Parse(Environment.GetEnvironmentVariable("FORCE_NEW_DEBUG_GAME") ?? "false");

        private GameCreatorService _gameCreatorService;
        private GameLoaderService _gameLoaderService;
        private ServerOptimizer _serverOptimizer;
        private DaemonService _daemonService;
        private bool _titleLaunched = false;


        public override void Entry(IModHelper helper)
        {
            Game1.options.pauseWhenOutOfFocus = false;

            var harmony = new Harmony(this.ModManifest.UniqueID);
            var daemonHttpClient = new HttpClient();
            daemonHttpClient.BaseAddress = new Uri($"http://localhost:{DaemonPort}");


            var options = new PersistentOptions(helper);
            var chatCommands = new ChatCommands(Monitor, harmony, helper);
            var cropSaver = new CropSaver(helper, harmony, Monitor);
            _serverOptimizer = new ServerOptimizer(harmony, Monitor, helper, DisableRendering);
            var alwaysOnServer = new AlwaysOnServer(helper, Monitor, chatCommands);
            _gameLoaderService = new GameLoaderService(helper, Monitor);
            _daemonService = new DaemonService(daemonHttpClient, Monitor);
            var backupService = new BackupService(daemonHttpClient, Monitor);
            var backupScheduler = new BackupScheduler(helper, backupService, Monitor);
            var gameTweaker = new GameTweaker(helper);
            var cabinManager = new CabinManagerService(helper, Monitor, harmony, CabinStrategy.FarmhouseStack);
            _gameCreatorService =
                new GameCreatorService(_gameLoaderService, options, Monitor, _daemonService, cabinManager);
            var networkTweaker = new NetworkTweaker(helper, options);
            var desyncKicker = new DesyncKicker(helper, Monitor);

            // var steamAuthHttpClient = new HttpClient();
            // steamAuthHttpClient.BaseAddress = new Uri($"http://{SteamAuthServerAddress}");
            // var steamAuthClient = new SteamAuthClient(steamAuthHttpClient, Monitor);
            // var galaxyAuthService = new GalaxyAuthService(Monitor, helper, harmony, steamAuthClient);

            var hostBot = new HostBot(helper, Monitor);
            // CabinCommand.Register(helper, chatCommands, options, Monitor);

            helper.Events.Display.RenderedActiveMenu += OnRenderedActiveMenu;
        }


        private void OnTitleMenuLaunched()
        {
            if (ForceNewDebugGame)
            {
                var config = new NewGameConfig
                {
                    WhichFarm = 0,
                    MaxPlayers = 4,
                };
                _gameCreatorService.CreateNewGame(config);
                return;
            }


            var successfullyStarted = true;
            if (_gameLoaderService.HasLoadableSave())
            {
                successfullyStarted = _gameLoaderService.LoadSave();
            }
            else
            {
                successfullyStarted = _gameCreatorService.CreateNewGameFromDaemonConfig();
            }

            try
            {
                if (successfullyStarted)
                {
                    var updateTask = _daemonService.UpdateConnectableStatus();
                    updateTask.Wait();
                }
                else
                {
                    var updateTask = _daemonService.UpdateNotConnectableStatus();
                    updateTask.Wait();
                }
            }
            catch (Exception e)
            {
                Monitor.Log(e.ToString(), LogLevel.Error);
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
    }
}