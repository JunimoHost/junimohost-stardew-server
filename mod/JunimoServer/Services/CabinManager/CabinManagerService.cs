using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Network;

namespace JunimoServer.Services.CabinManager
{
    public enum CabinStrategy
    {
        CabinStack,
        FarmhouseStack
    }

    public class CabinManagerData
    {
        public Vector2? DefaultCabinLocation = null;
        public HashSet<long> AllPlayerIdsEverJoined = new HashSet<long>();
    }


    public class CabinManagerService
    {
        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;
        private CabinManagerData data = new CabinManagerData();
        private const string CabinManagerDataKey = "JunimoHost.CabinManager.data";
        private const int MinEmptyCabins = 4;
        public const int HiddenCabinX = -20;
        public const int HiddenCabinY = -20;

        private readonly CabinStrategy _strategy;
        private readonly HashSet<long> farmersInFarmhouse = new HashSet<long>();

        public CabinManagerService(IModHelper helper, IMonitor monitor, Harmony harmony, CabinStrategy cabinStrategy,
            bool debug = false)
        {
            _helper = helper;
            _monitor = monitor;
            _strategy = cabinStrategy;
            CabinManagerOverrides.Initialize(helper, monitor, data, cabinStrategy, OnServerJoined);

            _helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            _helper.Events.GameLoop.UpdateTicked += OnTicked;
            if (debug)
            {
                harmony.Patch(
                    original: AccessTools.Method(typeof(Multiplayer), nameof(Multiplayer.processIncomingMessage)),
                    postfix: new HarmonyMethod(typeof(CabinManagerOverrides),
                        nameof(CabinManagerOverrides.processIncomingMessage_Postfix))
                );
            }


            harmony.Patch(
                original: AccessTools.Method(typeof(Multiplayer), "broadcastLocationMessage"),
                postfix: new HarmonyMethod(typeof(CabinManagerOverrides),
                    nameof(CabinManagerOverrides.broadcastLocationMessage_Postfix))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(GameServer), "sendLocation"),
                prefix: new HarmonyMethod(typeof(CabinManagerOverrides),
                    nameof(CabinManagerOverrides.sendLocation_Prefix))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(GameServer), "sendLocation"),
                postfix: new HarmonyMethod(typeof(CabinManagerOverrides),
                    nameof(CabinManagerOverrides.sendLocation_Postfix))
            );
            
            harmony.Patch(
                original: AccessTools.Method(typeof(GameServer), nameof(GameServer.sendServerIntroduction)),
                postfix: new HarmonyMethod(typeof(CabinManagerOverrides),
                    nameof(CabinManagerOverrides.sendServerIntroduction_Postfix))
            );
        }
        private void OnServerJoined(long peerId)
        {
            data.AllPlayerIdsEverJoined.Add(peerId);
            _helper.Data.WriteSaveData(CabinManagerDataKey, data);
            EnsureAtLeastXCabins();
        }


        private void OnTicked(object sender, UpdateTickedEventArgs e)
        {
            if (_strategy == CabinStrategy.FarmhouseStack)
            {
                MonitorFarmhouse();
            }
        }
        
        private void MonitorFarmhouse()
        {
            if (!Game1.hasLoadedGame) return;
            
            var idsInFarmHouse = Game1.getLocationFromName("Farmhouse").farmers
                .Select(farmer => farmer.UniqueMultiplayerID)
                .ToHashSet();

            foreach (var farmer in Game1.getLocationFromName("Farmhouse").farmers)
            {
                var id = farmer.UniqueMultiplayerID;
                if (!farmersInFarmhouse.Contains(id))
                {
                    farmersInFarmhouse.Add(id);
                    OnFarmerEnteredFarmhouse(farmer);
                }
            }

            farmersInFarmhouse.RemoveWhere(farmerId => !idsInFarmHouse.Contains(farmerId));
        }

        private void OnFarmerEnteredFarmhouse(Farmer farmer)
        {
            if (farmer.UniqueMultiplayerID == Game1.player.UniqueMultiplayerID) return;

            var farmersCabin = Game1.getFarm().buildings.First(building =>
                building.isCabin && ((Cabin) building.indoors.Value).owner.UniqueMultiplayerID ==
                farmer.UniqueMultiplayerID);
            var farmersCabinUniqueLocation = farmersCabin.nameOfIndoors;
            var farmersCabinEntrypoint = ((Cabin)farmersCabin.indoors.Value).getEntryLocation();
            
            // Pass out request
            Game1.server.sendMessage(farmer.UniqueMultiplayerID, 29, Game1.player, new object[]
            {
                farmersCabinUniqueLocation,
                farmersCabinEntrypoint.X,
                farmersCabinEntrypoint.Y,
                true
            });
        }

        private Vector2 GetStackLocation()
        {
            var initialCabin = Game1.getFarm().buildings.First(building => building.isCabin);
            var cabinLocation = new Vector2(initialCabin.tileX.Value, initialCabin.tileY.Value);
            var stackLocation = data.DefaultCabinLocation ?? cabinLocation;
            return stackLocation;
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            data = _helper.Data.ReadSaveData<CabinManagerData>(CabinManagerDataKey) ?? new CabinManagerData();
        }

        public void SetDefaultCabinLocation(Vector2 location)
        {
            data.DefaultCabinLocation = location;
            _helper.Data.WriteSaveData(CabinManagerDataKey, data);
        }

        public void MoveCabinToHiddenStack(Building cabin)
        {
            cabin.tileX.Value = HiddenCabinX;
            cabin.tileY.Value = HiddenCabinY;
        }

        public void EnsureAtLeastXCabins()
        {
            var cabinBlueprint = new BluePrint("Log Cabin");
            var outOfBoundsLocation = new Vector2(HiddenCabinX, HiddenCabinY);


            var numEmptyCabins = Game1.getFarm().buildings.Where(building => building.isCabin)
                .Count(cabin =>
                    !data.AllPlayerIdsEverJoined.Contains(
                        ((Cabin) cabin.indoors.Value).farmhand.Value.UniqueMultiplayerID)
                );


            var cabinsToBuild = MinEmptyCabins - numEmptyCabins;
            if (cabinsToBuild <= 0) return;

            for (var i = 0; i < cabinsToBuild; i++)
            {
                if (Game1.getFarm().buildStructure(cabinBlueprint, outOfBoundsLocation, Game1.player,
                        skipSafetyChecks: true))
                {
                    var cabin = Game1.getFarm().buildings.Last();
                    cabin.daysOfConstructionLeft.Value = 0;
                }
            }
        }
    }
}