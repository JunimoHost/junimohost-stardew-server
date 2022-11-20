﻿using System;
using System.Net.Http;
using Grpc.Net.Client;
using HarmonyLib;
using Junimohost.Stardewgame.V1;
using Junimohost.Stardewsteamauth.V1;
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
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace JunimoServer
{
    internal class ModEntry : Mod
    {

        private static readonly bool SteamAuthEnabled =
            bool.Parse(Environment.GetEnvironmentVariable("STEAM_AUTH_ENABLED") ?? "false");
        
        private static readonly string SteamAuthServerAddress =
            Environment.GetEnvironmentVariable("STEAM_AUTH_IP_PORT") ?? "localhost:50053";
        private static readonly string JunimoBootServerAddress =
            Environment.GetEnvironmentVariable("JUNIMO_BOOT_IP_PORT") ?? "";

        private static readonly int HealthCheckSeconds =
            Int32.Parse(Environment.GetEnvironmentVariable("HEALTH_CHECK_SECONDS") ?? "10");
        private static readonly string ServerId =
            Environment.GetEnvironmentVariable("SERVER_ID") ?? "test";
        
        private static readonly string DaemonPort = Environment.GetEnvironmentVariable("DAEMON_HTTP_PORT") ?? "8080";

        private static readonly bool DisableRendering =
            bool.Parse(Environment.GetEnvironmentVariable("DISABLE_RENDERING") ?? "true");

        private static readonly bool ForceNewDebugGame =
            bool.Parse(Environment.GetEnvironmentVariable("FORCE_NEW_DEBUG_GAME") ?? "false");

        private GameCreatorService _gameCreatorService;
        private GameLoaderService _gameLoaderService;
        private ServerOptimizer _serverOptimizer;
        private DaemonService _daemonService;
        private StardewGameService.StardewGameServiceClient _stardewGameServiceClient;
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
            var cabinManager = new CabinManagerService(helper, Monitor, harmony, options);
            _gameCreatorService =
                new GameCreatorService(_gameLoaderService, options, Monitor, _daemonService, cabinManager);
            var networkTweaker = new NetworkTweaker(helper, options);
            var desyncKicker = new DesyncKicker(helper, Monitor);

            if (SteamAuthEnabled)
            {
                var steamTicketGenChannel = GrpcChannel.ForAddress($"http://{SteamAuthServerAddress}");
                var steamTicketGenClient = new StardewSteamAuthService.StardewSteamAuthServiceClient(steamTicketGenChannel);
                var galaxyAuthService = new GalaxyAuthService(Monitor, helper, harmony, steamTicketGenClient);
                
                var junimoBootGenChannel = GrpcChannel.ForAddress($"http://{JunimoBootServerAddress}");
                _stardewGameServiceClient = new StardewGameService.StardewGameServiceClient(junimoBootGenChannel);
            }

            var hostBot = new HostBot(helper, Monitor);
            CabinCommand.Register(helper, chatCommands, options, Monitor);

            helper.Events.Display.RenderedActiveMenu += OnRenderedActiveMenu;
            helper.Events.GameLoop.OneSecondUpdateTicked += OnOneSecondTicked;
        }

        private int _healthCheckTimer = 0;
        private void OnOneSecondTicked(object sender, OneSecondUpdateTickedEventArgs e)
        {
            _healthCheckTimer--;
            if (Game1.server != null)
            {
                if (Game1.server.getInviteCode() != null && _healthCheckTimer == 0)
                {
                    Monitor.Log(Game1.server.getInviteCode(), LogLevel.Info);
                    _stardewGameServiceClient.GameHealthCheckAsync(new GameHealthCheckRequest
                    {
                        InviteCode = Game1.server.getInviteCode(),
                        IsConnectable = true,
                        ServerId = ServerId,
                    });
                    _healthCheckTimer = HealthCheckSeconds;

                }
                else
                {
                    Monitor.Log("Invite code is null", LogLevel.Info);

                }
            }
            else
            {
                Monitor.Log("Waiting for server to not be null", LogLevel.Info);
            }
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