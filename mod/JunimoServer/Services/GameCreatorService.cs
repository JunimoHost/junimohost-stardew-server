using StardewModdingAPI;
using StardewValley;
using System;

namespace JunimoServer.Services
{
    public record NewGameConfig(int WhichFarm = 0, bool UseSeperateWallets = false, int StartingCabins = 1);

    class GameCreatorService
    {

        private IModHelper helper;
        public GameCreatorService(IModHelper helper) {
            this.helper = helper;
        }

        public void createNewGame(NewGameConfig config) {

            Game1.player.team.useSeparateWallets.Value = config.UseSeperateWallets;
            Game1.startingCabins = config.StartingCabins;
            Game1.whichFarm = config.WhichFarm;
            bool isWildernessFarm = config.WhichFarm == 4;
            if (isWildernessFarm) {

            }
        }

    }
}
