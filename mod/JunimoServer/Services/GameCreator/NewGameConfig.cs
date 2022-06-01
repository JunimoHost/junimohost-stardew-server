﻿using System;

namespace JunimoServer.Services.GameCreator
{
    public class NewGameConfig
    {
        public int WhichFarm { get; set; } = 0;
        public bool UseSeparateWallets { get; set; } = false;
        public int StartingCabins { get; set; } = 1;
        public bool CatPerson { get; set; } = false;
        public string FarmName { get; set; } = "Junimo";

        public int MaxPlayers { get; set; } = 4;

        //public static NewGameConfig FromCreateGameRequest(CreateGameRequest req) {
        //    return new NewGameConfig { 
        //        WhichFarm = req.WhichFarm,
        //        UseSeperateWallets = req.UseSeperateWallets,
        //        StartingCabins = req.StartingCabins,
        //        CatPerson = req.CatPerson,
        //        FarmName = req.FarmName,
        //    };
        //}

        public override string ToString()
        {
            return
                $"{nameof(WhichFarm)}: {WhichFarm}, {nameof(UseSeparateWallets)}: {UseSeparateWallets}, {nameof(StartingCabins)}: {StartingCabins}, {nameof(CatPerson)}: {CatPerson}, {nameof(FarmName)}: {FarmName}, {nameof(MaxPlayers)}: {MaxPlayers}";
        }
    }
}