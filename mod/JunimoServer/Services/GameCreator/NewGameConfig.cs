using JunimoServer.GRPC;
using System;

namespace JunimoServer.Services.GameCreator
{
    public class NewGameConfig
    {
        public int WhichFarm { get; set; } = 0;
        public bool UseSeperateWallets { get; set; } = false;
        public int StartingCabins { get; set; } = 1;
        public bool CatPerson { get; set; } = false;
        public string FarmName { get; set; } = "Junimo";

        public static NewGameConfig FromCreateGameRequest(CreateGameRequest req) {
            return new NewGameConfig { 
                WhichFarm = req.WhichFarm,
                UseSeperateWallets = req.UseSeperateWallets,
                StartingCabins = req.StartingCabins,
                CatPerson = req.CatPerson,
                FarmName = req.FarmName,
            };
        }

    }
}
