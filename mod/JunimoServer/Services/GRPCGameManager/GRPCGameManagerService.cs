//using Grpc.Core;
//using JunimoServer.GRPC;
//using Google.Protobuf.WellKnownTypes;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using StardewModdingAPI;
//using JunimoServer.Services.GameLoader;
//using JunimoServer.Services.GameCreator;
//using System.Text.RegularExpressions;
//using StardewValley;

//namespace JunimoServer.Services.GRPCGameManager
//{
//    class GRPCGameManagerService : GameManagerService.GameManagerServiceBase
//    {
//        private readonly GameCreatorService gameCreator;
//        private readonly GameLoaderService gameLoader;
//        private readonly IMonitor monitor;

//        public GRPCGameManagerService(GameCreatorService gameCreator, GameLoaderService gameLoader, IMonitor monitor)
//        {
//            this.gameCreator = gameCreator;
//            this.gameLoader = gameLoader;
//            this.monitor = monitor;
//        }

//        public override Task<Empty> CreateGame(CreateGameRequest request, ServerCallContext context)
//        {
//            monitor.Log("creating GRPC createGame", LogLevel.Info);
//            NewGameConfig config = NewGameConfig.FromCreateGameRequest(request);
//            gameCreator.CreateNewGame(config);

//            // Save name goes from nothing -> _uuid -> farmName_uuid (once loaded in completely)
//            string saveName = Constants.SaveFolderName;
//            if (Constants.SaveFolderName.Substring(0, 1) == "_") // savename is in midpoint
//            {
//                saveName = SaveGame.FilterFileName(config.FarmName) + saveName;
//            } 

//            gameLoader.SetSaveNameToLoad(saveName);
//            monitor.Log("created GRPC createGame", LogLevel.Info);

//            return Task.FromResult(new Empty());
//        }
//    }
//}
