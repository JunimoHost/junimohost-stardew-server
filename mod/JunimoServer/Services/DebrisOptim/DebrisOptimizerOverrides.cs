using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;

namespace JunimoServer.Services.DebrisOptim
{
    public class DebrisOptimizerOverrides
    {
        private static IMonitor _monitor;
        private static IModHelper _helper;
        private static Queue<Debris> _debrisQueue = new Queue<Debris>();
        private static HashSet<Debris> _debrisSet = new HashSet<Debris>();
        private static int _maxUpdatesPerTick = 2;

        public static void Initialize(IMonitor monitor, IModHelper helper)
        {
            _monitor = monitor;
            _helper = helper;
        }

        // ReSharper disable once InconsistentNaming
        public static bool updateChunks_Prefix(Debris __instance, GameTime time, GameLocation location)
        {
            if (__instance.debrisType.Value != Debris.DebrisType.RESOURCE) return true;

            if (!_debrisSet.Contains(__instance))
            {
                _debrisQueue.Enqueue(__instance);
                _debrisSet.Add(__instance);
            }
            return false;
        }

        public static void Update()
        {
            int updatesThisTick = 0;

            while (_debrisQueue.Count > 0 && updatesThisTick < _maxUpdatesPerTick)
            {
                var debris = _debrisQueue.Dequeue();
                _debrisSet.Remove(debris);
                var farmer = _helper.Reflection.GetMethod(debris, "findBestPlayer").Invoke<Farmer>(Game1.currentLocation);

                for (int index = debris.Chunks.Count - 1; index >= 0; --index)
                {
                    var chunk = debris.Chunks[index];
                    var farmerCanAccept = farmer.couldInventoryAcceptThisObject(chunk.debrisType - chunk.debrisType % 2, 1);
                    if (farmerCanAccept)
                    {
                        var collectSucceeded = debris.collect(farmer, chunk);
                        if (collectSucceeded)
                        {
                            debris.Chunks.RemoveAt(index);
                        }
                    }
                }

                updatesThisTick++;
            }
        }
    }
}