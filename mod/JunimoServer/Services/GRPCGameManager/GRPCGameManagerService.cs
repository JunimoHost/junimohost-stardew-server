using Grpc.Core;
using JunimoServer.GRPC;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;
using JunimoServer.Services.GameLoader;
using JunimoServer.Services.GameCreator;
using System.Text.RegularExpressions;
using StardewValley;

namespace JunimoServer.Services.GRPCGameManager
{
    class GRPCGameManagerService : GameManagerService.GameManagerServiceBase
    {
        private readonly GameCreatorService gameCreator;
        private readonly GameLoaderService gameLoader;

        public GRPCGameManagerService(GameCreatorService gameCreator, GameLoaderService gameLoader)
        {
            this.gameCreator = gameCreator;
            this.gameLoader = gameLoader;
        }

        public override Task<Empty> CreateGame(CreateGameRequest request, ServerCallContext context)
        {
            NewGameConfig config = NewGameConfig.FromCreateGameRequest(request);
            gameCreator.CreateNewGame(config);

            // Save name goes from nothing -> _uuid -> farmName_uuid (once loaded in completely)
            string saveName = Constants.SaveFolderName;
            if (Constants.SaveFolderName.Substring(0, 1) == "_") // savename is in midpoint
            {
                saveName = SaveGame.FilterFileName(config.FarmName) + saveName;
            } 

            gameLoader.SetSaveNameToLoad(saveName);

            return Task.FromResult(new Empty());
        }
    }
}
