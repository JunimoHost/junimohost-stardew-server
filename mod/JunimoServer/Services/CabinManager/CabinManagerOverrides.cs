using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Network;

namespace JunimoServer.Services.CabinManager
{
    public class CabinManagerOverrides
    {
        private static IMonitor _monitor;
        private static IModHelper _helper;
        private static CabinManagerData _cabinManagerData;
        private static CabinStrategy _strategy;

        public static void Initialize(IModHelper helper, IMonitor monitor, CabinManagerData cabinManagerData,
            CabinStrategy strategy)
        {
            _helper = helper;
            _monitor = monitor;
            _cabinManagerData = cabinManagerData;
            _strategy = strategy;
        }


        public static void processIncomingMessage_Postfix(Multiplayer __instance, IncomingMessage msg)
        {
            _monitor.Log($"Called processIncomingMessage_Prefix type: {msg.MessageType}");

            var allCabins = Game1.getFarm().buildings.Where(building =>
                building.isCabin).ToArray();

            foreach (var cabins in allCabins)
            {
                var indoor = (Cabin) cabins.indoors.Value;
                _monitor.Log($"cabin: {indoor.NameOrUniqueName}");
                foreach (var warp in indoor.warps)
                {
                    _monitor.Log($"warp: {warp.TargetX},{warp.TargetY}");
                }
            }

            if (msg.MessageType != 8)
            {
                return;
            }

            NPC character = _helper.Reflection.GetMethod(__instance, "readNPC").Invoke<NPC>(msg.Reader);
            GameLocation location = _helper.Reflection.GetMethod(__instance, "readLocation")
                .Invoke<GameLocation>(msg.Reader);


            if (character != null && location != null)
            {
                var tileVec = msg.Reader.ReadVector2();
                _monitor.Log($"loc {location.NameOrUniqueName} tile: {tileVec.X}, {tileVec.Y}");

                Game1.warpCharacter(character, location, tileVec);
            }

            // _monitor.Log($"Farm has # buildings: {Game1.getFarm().buildings.Count}");
        }

        public class OriginalBuildingCoord
        {
            public Building Building;
            public int X;
            public int Y;

            public OriginalBuildingCoord(Building building, int x, int y)
            {
                Building = building;
                X = x;
                Y = y;
            }
        }

        public static Building[] GetCabinsToMove(long playerId, Vector2? cabinStackLocation)
        {
            var farm = Game1.getFarm();
            var buildings = farm.buildings;
            var allCabins = buildings.Where(building =>
                building.isCabin).ToArray();


            var cabinsToMove = allCabins
                .Where(building => ((Cabin) building.indoors.Value).owner.UniqueMultiplayerID == playerId)
                .Where(building => building.tileX.Value == CabinManagerService.HiddenCabinX
                                   && building.tileY.Value == CabinManagerService.HiddenCabinY)
                .ToArray();

            return cabinsToMove;
        }

        public static List<OriginalBuildingCoord> MoveCabinsAway(long playerId, Vector2? cabinStackLocation)
        {
            var farm = Game1.getFarm();
            var movedBuildings = new List<OriginalBuildingCoord>();
            var cabinsToMove = GetCabinsToMove(playerId, cabinStackLocation);


            if (_strategy == CabinStrategy.FarmhouseStack)
            {
                foreach (var building in cabinsToMove)
                {
                    var original = new OriginalBuildingCoord(building, building.tileX.Value, building.tileY.Value);
                    movedBuildings.Add(original);
                    var indoor = (Cabin) building.indoors.Value;
                    foreach (var warp in indoor.warps)
                    {
                        warp.TargetX = farm.GetMainFarmHouseEntry().X;
                        warp.TargetY = farm.GetMainFarmHouseEntry().Y;
                    }
                }

                return movedBuildings;
            }
            // CabinStack Strategy

            var stackX = cabinStackLocation?.X != null
                ? (int) Math.Round(cabinStackLocation.Value.X, 0)
                : 20;
            var stackY = cabinStackLocation?.Y != null
                ? (int) Math.Round(cabinStackLocation.Value.Y, 0)
                : 20;

            foreach (var building in cabinsToMove)
            {
                var original = new OriginalBuildingCoord(building, building.tileX.Value, building.tileY.Value);
                movedBuildings.Add(original);

                building.tileX.Value = stackX;
                building.tileY.Value = stackY;

                var indoor = (Cabin) building.indoors.Value;
                foreach (var warp in indoor.warps)
                {
                    warp.TargetX = stackX + 2;
                    warp.TargetY = stackY + 2;
                }
            }

            return movedBuildings;
        }

        public static void MoveCabinsHome(List<OriginalBuildingCoord> originalBuildingCoords)
        {
            if (_strategy == CabinStrategy.CabinStack)
            {
                foreach (var originalBuildingCoord in originalBuildingCoords)
                {
                    var building = originalBuildingCoord.Building;
                    var x = originalBuildingCoord.X;
                    var y = originalBuildingCoord.Y;
                    building.tileX.Value = x;
                    building.tileY.Value = y;
                }
            }
        }

        public static void sendLocation_Prefix(GameServer __instance, long peer, GameLocation location,
            bool force_current, out List<OriginalBuildingCoord> __state)
        {
            __state = MoveCabinsAway(peer, _cabinManagerData.DefaultCabinLocation);
        }

        public static void sendLocation_Postfix(GameServer __instance, long peer, GameLocation location,
            bool force_current, List<OriginalBuildingCoord> __state)
        {
            MoveCabinsHome(__state);
        }


        public static void broadcastLocationMessage_Postfix(Multiplayer __instance, GameLocation loc,
            OutgoingMessage message)
        {
            if (loc.Name.StartsWith("Cabin"))
            {
                _monitor.Log("cabin info");
            }

            if (loc.Name is not "Farm" || loc.Name.StartsWith("Cabin"))
            {
                return;
            }

            if (Game1.IsClient)
            {
                Game1.client.sendMessage(message);
                return;
            }

            Action<Farmer> tellFarmer = delegate(Farmer f)
            {
                if (f != Game1.player)
                {
                    var farm = Game1.getFarm();


                    var movedBuildings = MoveCabinsAway(f.UniqueMultiplayerID, _cabinManagerData.DefaultCabinLocation);

                    byte[] data = __instance.writeObjectDeltaBytes(farm.Root as NetRoot<GameLocation>);

                    MoveCabinsHome(movedBuildings);

                    OutgoingMessage forgedMessage = new OutgoingMessage(6, Game1.player, new object[]
                    {
                        loc.isStructure.Value,
                        loc.isStructure ? loc.uniqueName.Value : loc.name.Value,
                        data
                    });


                    Game1.server.sendMessage(f.UniqueMultiplayerID, forgedMessage);
                }
            };
            if (__instance.isAlwaysActiveLocation(loc))
            {
                using (IEnumerator<Farmer> enumerator = Game1.otherFarmers.Values.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        Farmer farmer = enumerator.Current;
                        tellFarmer(farmer);
                    }

                    return;
                }
            }

            foreach (Farmer f3 in loc.farmers)
            {
                tellFarmer(f3);
            }

            if (loc is BuildableGameLocation)
            {
                foreach (Building building in (loc as BuildableGameLocation).buildings)
                {
                    if (building.indoors.Value != null)
                    {
                        foreach (Farmer f2 in building.indoors.Value.farmers)
                        {
                            tellFarmer(f2);
                        }
                    }
                }
            }
        }
    }
}