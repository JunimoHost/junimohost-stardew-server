using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;

namespace JunimoServer.Services.ServerOptimizer
{
    public class ServerOptimizerOverrides
    {
        private static IMonitor _monitor;
        private static bool _disabled = true;

        public static void Initialize(IMonitor monitor)
        {
            _monitor = monitor;
        }

        public static bool Draw_Prefix(GameRunner __instance)

        {
            if (!_disabled)
            {
                __instance.SuppressDraw();
            }

            return _disabled;
        }

        public static void DisableDrawing()
        {
            _disabled = false;
        }
    }
}