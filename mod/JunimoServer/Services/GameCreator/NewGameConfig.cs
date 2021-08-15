using System;

namespace JunimoServer.Services.GameCreator
{
    public class NewGameConfig
    {
        public int WhichFarm { get; set; } = 3;
        public bool UseSeperateWallets { get; set; } = false;
        public int StartingCabins { get; set; } = 1;
        public bool CatPerson { get; set; } = false;
        public string FarmName { get; set; } = "Junimo";

    }
}
