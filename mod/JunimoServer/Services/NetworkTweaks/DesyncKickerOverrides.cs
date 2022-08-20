using System.Threading;
using JunimoServer.Util;
using StardewModdingAPI;
using StardewValley;
using StardewValley.SDKs;

namespace JunimoServer.Services.NetworkTweaks
{
    public class DesyncKickerOverrides
    {
        private static IMonitor _monitor;
        private static IModHelper _helper;

        public static void Initialize(IMonitor monitor, IModHelper helper)
        {
            _monitor = monitor;
            _helper = helper;
        }

        public static bool NewDaySynchronizer_processMessages_prefix()
        {
            var multiplayer = _helper.GetMultiplayer();
            var sdk = _helper.Reflection.GetField<SDKHelper>(typeof(Program), "_sdk").GetValue();

            _monitor.Log("Updating Late");
            multiplayer.UpdateLate();
            _monitor.Log("Sleep");

            Thread.Sleep(16);
            _monitor.Log("SDK Update");

            sdk.Update();

            _monitor.Log("UpdateEarly");
            multiplayer.UpdateEarly();
            return false;
        }
    }
}