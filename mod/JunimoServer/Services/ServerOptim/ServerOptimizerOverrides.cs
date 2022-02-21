using StardewModdingAPI;
using StardewValley;

namespace JunimoServer.Services.ServerOptim
{
    public class ServerOptimizerOverrides
    {
        private static IMonitor _monitor;
        private static bool _shouldDrawFrame = true;

        public static void Initialize(IMonitor monitor)
        {
            _monitor = monitor;
        }

        public static bool Draw_Prefix(GameRunner __instance)

        {
            if (!_shouldDrawFrame)
            {
                __instance.SuppressDraw();
            }

            return _shouldDrawFrame;
        }

        public static void DisableDrawing()
        {
            _shouldDrawFrame = false;
        }

        public static void EnableDrawing()
        {
            _shouldDrawFrame = true;
        }
    }
}