using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;

namespace JunimoServer.Services.GameCreator
{


    class GameCreatorService
    {

        public GameCreatorService() {
        }

        public void CreateNewGame(NewGameConfig config)
        {

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

            if (Game1.activeClickableMenu is TitleMenu menu)
            {
                menu.createdNewCharacter(skipIntro: true);
            }
        }

    }
}
