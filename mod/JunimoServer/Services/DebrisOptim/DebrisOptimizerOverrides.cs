using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;
using Netcode;

namespace JunimoServer.Services.DebrisOptim
{
    public class DebrisOptimizerOverrides
    {
        private static IMonitor _monitor;
        private static IModHelper _helper;
        private static Queue<DebrisLocation> _debrisQueue = new Queue<DebrisLocation>();
        private static HashSet<Debris> _debrisSet = new HashSet<Debris>();
        private static int _maxUpdatesPerTick = 2;

        public class DebrisLocation
        {
            public Debris Debris { get; set; }
            public GameLocation Location { get; set; }

            public DebrisLocation(Debris debris, GameLocation location)
            {
                Debris = debris;
                Location = location;
            }
        }


        public static void Initialize(IMonitor monitor, IModHelper helper)
        {
            _monitor = monitor;
            _helper = helper;
        }

        // ReSharper disable once InconsistentNaming
        public static bool updateChunks_Prefix(Debris __instance, GameTime time, GameLocation location,
            ref bool __result)
        {
            if (__instance.debrisType.Value != Debris.DebrisType.RESOURCE) return true;

            __result = false;
            if (!_debrisSet.Contains(__instance))
            {
                _debrisQueue.Enqueue(new DebrisLocation(__instance, location));
                _debrisSet.Add(__instance);
            }

            return false;
        }

        public static void Update()
        {
            int updatesThisTick = 0;

            while (_debrisQueue.Count > 0 && updatesThisTick < _maxUpdatesPerTick)
            {
                var debrisLocation = _debrisQueue.Dequeue();
                var debris = debrisLocation.Debris;
                var location = debrisLocation.Location;
                _debrisSet.Remove(debris);

                Vector2 debrisPos = new Vector2();
                foreach (Chunk chunk in debris.Chunks)
                {
                    debrisPos += chunk.position.Value;
                }

                debrisPos /= debris.Chunks.Count;

                Farmer closestFarmer = null;
                var closestDistance = float.MaxValue;
                foreach (Farmer farmer in location.farmers)
                {
                    var distance = (farmer.Position - debrisPos).LengthSquared();
                    if (distance < closestDistance)
                    {
                        closestFarmer = farmer;
                        closestDistance = distance;
                    }
                }

                if (closestFarmer == null)
                {
                    if (!_debrisSet.Contains(debris))
                    {
                        _debrisQueue.Enqueue(new DebrisLocation(debris, location));
                        _debrisSet.Add(debris);
                    }

                    return;
                }


                for (int index = debris.Chunks.Count - 1; index >= 0; --index)
                {
                    var chunk = debris.Chunks[index];

                    int itemId =
                        (Debris.DebrisType)(NetFieldBase<Debris.DebrisType, NetEnum<Debris.DebrisType>>)debris
                            .debrisType == Debris.DebrisType.ARCHAEOLOGY ||
                        (Debris.DebrisType)(NetFieldBase<Debris.DebrisType, NetEnum<Debris.DebrisType>>)debris
                            .debrisType == Debris.DebrisType.OBJECT
                            ? chunk.debrisType
                            : chunk.debrisType - chunk.debrisType % 2;

                    Item item;
                    if (chunk.debrisType != 93 && chunk.debrisType != 94)
                        item = new Object(Vector2.Zero, itemId, 1)
                        {
                            Quality = debris.itemQuality,
                        };
                    else
                        item = new Torch(Vector2.Zero, 1, itemId);
                    var farmerCanAccept =
                        closestFarmer.couldInventoryAcceptThisItem(item);
                    if (farmerCanAccept)
                    {
                        closestFarmer.addItemToInventory(item);
                    }
                }

                updatesThisTick++;
            }
        }
    }
}