﻿using System;
using System.Linq;
using System.Threading;
using JunimoServer.Services.CabinManager;
using JunimoServer.Services.Daemon;
using JunimoServer.Services.GameLoader;
using JunimoServer.Services.PersistentOption;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;

namespace JunimoServer.Services.GameCreator
{
    class GameCreatorService
    {
        private readonly DaemonService _daemonService;
        private readonly CabinManagerService _cabinManagerService;
        private readonly GameLoaderService _gameLoader;
        private static readonly Mutex CreateGameMutex = new Mutex();
        private readonly PersistentOptions _options;
        private readonly IMonitor _monitor;
        private readonly IModHelper _helper;

        public bool GameIsCreating { get; private set; }

        public GameCreatorService(GameLoaderService gameLoader, PersistentOptions options, IMonitor monitor,
            DaemonService daemonService, CabinManagerService cabinManagerService, IModHelper helper)
        {
            _daemonService = daemonService;
            _options = options;
            _gameLoader = gameLoader;
            _monitor = monitor;
            _cabinManagerService = cabinManagerService;
            _helper = helper;
        }

        public bool CreateNewGameFromDaemonConfig()
        {
            try
            {
                var configTask = _daemonService.GetConfig();
                configTask.Wait();
                var config = configTask.Result;

                CreateNewGame(config);
                return true;
            }
            catch (Exception e)
            {
                _monitor.Log(e.ToString(), LogLevel.Error);
                return false;
            }
        }

        public void CreateNewGame(NewGameConfig config)
        {
            CreateGameMutex.WaitOne(); //prevent trying to start new game while in the middle of creating a game
            GameIsCreating = true;

            _options.SetPersistentOptions(new PersistentOptionsSaveData
            {
                MaxPlayers = config.MaxPlayers,
                CabinStrategy = (CabinStrategy)config.CabinStrategy
            });


            Game1.player.team.useSeparateWallets.Value = config.UseSeparateWallets;
            Game1.startingCabins = 1;

            var isUltimateFarmModLoaded = _helper.ModRegistry.GetAll().Any(mod => mod.Manifest.Name == "Ultimate Farm CP");
            if (isUltimateFarmModLoaded)
            {
                Game1.whichFarm = 1; // Ultimate Farm CP expects riverland == 1
            }
            else
            {
                Game1.whichFarm = config.WhichFarm;
            }
            
            var isWildernessFarm = config.WhichFarm == 4;
            Game1.spawnMonstersAtNight = isWildernessFarm;

            Game1.player.catPerson = config.CatPerson;

            Game1.player.Name = "junimohost";
            Game1.player.displayName = Game1.player.Name;
            Game1.player.favoriteThing.Value = "Junimos";
            Game1.player.farmName.Value = config.FarmName;

            Game1.player.isCustomized.Value = true;
            Game1.player.ConvertClothingOverrideToClothesItems();

            Game1.multiplayerMode = 2; // multiplayer enabled

            // From TitleMenu.createdNewCharacter
            Game1.loadForNewGame(false);
            Game1.saveOnNewDay = true;
            Game1.player.eventsSeen.Add(60367);
            Game1.player.currentLocation = Utility.getHomeOfFarmer(Game1.player);
            Game1.player.Position = new Vector2(9f, 9f) * 64f;
            Game1.player.isInBed.Value = true;
            Game1.NewDay(0f);
            Game1.exitActiveMenu();
            Game1.setGameMode(3);

            _gameLoader.SetCurrentGameAsSaveToLoad(config.FarmName);



            var initialCabin = Game1.getFarm().buildings.First(building => building.isCabin);
            var cabinLocation = new Vector2(initialCabin.tileX.Value, initialCabin.tileY.Value);

            _cabinManagerService.SetDefaultCabinLocation(cabinLocation);
            _cabinManagerService.MoveCabinToHiddenStack(initialCabin);


            GameIsCreating = false;
            CreateGameMutex.ReleaseMutex();
        }
    }
}