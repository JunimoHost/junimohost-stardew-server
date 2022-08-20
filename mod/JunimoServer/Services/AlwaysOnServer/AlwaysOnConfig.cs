using StardewModdingAPI;

namespace JunimoServer.Services.AlwaysOnServer
{
    class AlwaysOnConfig
    {
        public SButton ServerHotKey { get; set; } = SButton.F9;

        public int UpgradeHouse { get; set; } = 0;
        public string PetName { get; set; } = "Apples";
        public bool FarmCaveChoiceIsMushrooms { get; set; } = true;
        public bool IsCommunityCenterRun { get; set; } = true;

        public bool LockPlayerChests { get; set; } = true;

        public int EggHuntCountDownConfig { get; set; } = 300;
        public int FlowerDanceCountDownConfig { get; set; } = 300;
        public int LuauSoupCountDownConfig { get; set; } = 300;
        public int JellyDanceCountDownConfig { get; set; } = 300;
        public int GrangeDisplayCountDownConfig { get; set; } = 300;
        public int IceFishingCountDownConfig { get; set; } = 300;

        public int EndOfDayTimeOut { get; set; } = 60;
        public int FairTimeOut { get; set; } = 1200;
        public int SpiritsEveTimeOut { get; set; } = 900;
        public int WinterStarTimeOut { get; set; } = 900;

        public int EggFestivalTimeOut { get; set; } = 120;
        public int FlowerDanceTimeOut { get; set; } = 120;
        public int LuauTimeOut { get; set; } = 120;
        public int DanceOfJelliesTimeOut { get; set; } = 120;
        public int FestivalOfIceTimeOut { get; set; } = 120;

    }
}
