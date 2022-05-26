using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using JunimoServer.Services.Daemon;
using JunimoServer.Services.GameLoader;
using JunimoServer.Services.PersistentOption;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace JunimoServer.Services.GameCreator
{
    class GameCreatorService
    {
        private readonly DaemonService _daemonService;
        private readonly GameLoaderService _gameLoader;
        private static readonly Mutex CreateGameMutex = new Mutex();
        private readonly PersistentOptions _options;
        private readonly IMonitor _monitor;

        public bool GameIsCreating { get; private set; }

        public GameCreatorService(GameLoaderService gameLoader, PersistentOptions options, IMonitor monitor,
            DaemonService daemonService)
        {
            _daemonService = daemonService;
            _options = options;
            _gameLoader = gameLoader;
            _monitor = monitor;
        }

        public async void CreateNewGameFromDaemonConfig()
        {
            try
            {
                var config = await _daemonService.GetConfig();
                CreateNewGame(config);
            }
            catch (Exception e)
            {
                _monitor.Log(e.ToString(), LogLevel.Error);
                await _daemonService.UpdateNotConnectableStatus();
                return;
            }

            try
            {
                await _daemonService.UpdateConnectableStatus();
            }
            catch (Exception e)
            {
                _monitor.Log(e.ToString(), LogLevel.Error);
            }
        }

        private void CreateNewGame(NewGameConfig config)
        {
            CreateGameMutex.WaitOne(); //prevent trying to start new game while in the middle of creating a game
            GameIsCreating = true;

            _options.SetPersistentOptions(new PersistentOptionsSaveData
            {
                MaxPlayers = config.MaxPlayers
            });


            Game1.player.team.useSeparateWallets.Value = config.UseSeparateWallets;
            Game1.startingCabins = config.StartingCabins;

            Game1.whichFarm = config.WhichFarm;
            var isWildernessFarm = config.WhichFarm == 4;
            Game1.spawnMonstersAtNight = isWildernessFarm;

            Game1.player.catPerson = config.CatPerson;

            Game1.player.Name = "Host";
            Game1.player.displayName = Game1.player.Name;
            Game1.player.favoriteThing.Value = "Junimos";
            Game1.player.farmName.Value = config.FarmName;

            Game1.player.isCustomized.Value = true;
            Game1.player.ConvertClothingOverrideToClothesItems();

            Game1.multiplayerMode = 2; // multiplayer enabled

            // May not be needed
            // if (Game1.activeClickableMenu is IClickableMenu menu && menu != null)
            // {
            //     menu.exitThisMenuNoSound();
            // }

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

            GameIsCreating = false;
            CreateGameMutex.ReleaseMutex();
        }
    }
}