using JunimoServer.Services.GameCreator;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JunimoServer.Controllers
{
    class GameCreatorController
    {
        private IMonitor monitor;
        private GameCreatorService gameCreator;
        public GameCreatorController(IMonitor monitor, GameCreatorService gameCreator) {
            this.monitor = monitor;
            this.gameCreator = gameCreator;
        }

        public void CreateGame(string body)
        {
            monitor.Log("creating createGame", LogLevel.Info);

            NewGameConfig config = JsonConvert.DeserializeObject<NewGameConfig>(body);

            gameCreator.CreateNewGame(config);

            monitor.Log("created createGame", LogLevel.Info);

        }
    }
}
