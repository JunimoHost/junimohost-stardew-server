using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HarmonyLib;
using JunimoServer.Services;
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
using JunimoServer.Services.PersistentOption;
using JunimoServer.Services.ServerOptim;
using JunimoServer.Util;
using SimpleHttp;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Network;
using xTile.Dimensions;
using xTile.ObjectModel;

namespace JunimoServer
{
    internal class ModEntry : Mod
    {
        private static readonly string DaemonPort = Environment.GetEnvironmentVariable("DAEMON_HTTP_PORT") ?? "8080";
        private const bool DisableRendering = true;

        private GameCreatorService _gameCreatorService;
        private GameLoaderService _gameLoaderService;
        private ServerOptimizer _serverOptimizer;
        private DaemonService _daemonService;
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
            _daemonService = new DaemonService(httpClient, Monitor);
            var backupService = new BackupService(httpClient, Monitor);
            var backupScheduler = new BackupScheduler(helper, backupService, Monitor);
            var gameTweaker = new GameTweaker(helper);
            var cabinManager = new CabinManagerService(helper, Monitor, harmony, CabinStrategy.FarmhouseStack);
            _gameCreatorService = new GameCreatorService(_gameLoaderService, options, Monitor, _daemonService, cabinManager);
            // var galaxyAuthService = new GalaxyAuthService(Monitor, helper, harmony);

            CabinCommand.Register(helper, chatCommands, options, Monitor);

            helper.Events.Display.RenderedActiveMenu += OnRenderedActiveMenu;

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
            // var config = new NewGameConfig
            // {
            //     WhichFarm = 0,
            //     MaxPlayers = 9999,
            // };
            // _gameCreatorService.CreateNewGame(config);

            bool successfullyStarted = true;
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
    }
}