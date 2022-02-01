using JunimoServer.Services.GameLoader;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Threading;

namespace JunimoServer.Services.GameCreator
{


    class GameCreatorService
    {
        private readonly GameLoaderService _gameLoader;
        private static readonly Mutex CreateGameMutex = new Mutex();
        public GameCreatorService(GameLoaderService gameLoader) {
            this._gameLoader = gameLoader;
        }

        public void CreateNewGame(NewGameConfig config)
        {
            CreateGameMutex.WaitOne(); //prevent trying to start new game while in the middle of creating a game

            Game1.player.team.useSeparateWallets.Value = config.UseSeperateWallets;
            Game1.startingCabins = config.StartingCabins;

            Game1.whichFarm = config.WhichFarm;
            bool isWildernessFarm = config.WhichFarm == 4;
            Game1.spawnMonstersAtNight = isWildernessFarm;

            Game1.player.catPerson = config.CatPerson;

            Game1.player.Name = "Host";
            Game1.player.displayName = Game1.player.Name;
            Game1.player.favoriteThing.Value = "Junimos";
            Game1.player.farmName.Value = config.FarmName;

            Game1.player.isCustomized.Value = true;
            Game1.player.ConvertClothingOverrideToClothesItems();

            Game1.multiplayerMode = 2; // multiplayer enabled

            if (Game1.activeClickableMenu is IClickableMenu menu && menu != null)
            {
                menu.exitThisMenuNoSound();
            }

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
            
            CreateGameMutex.ReleaseMutex();
        }

    }
}
