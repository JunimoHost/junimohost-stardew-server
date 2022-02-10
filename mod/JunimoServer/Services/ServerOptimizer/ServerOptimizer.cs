using HarmonyLib;
using StardewModdingAPI;
using StardewValley;

namespace JunimoServer.Services.ServerOptimizer
{
    public class ServerOptimizer
    {
        public ServerOptimizer(Harmony harmony, IMonitor monitor)
        {
            ServerOptimizerOverrides.Initialize(monitor);
            harmony.Patch(
                original: AccessTools.Method(typeof(GameRunner), "BeginDraw"),
                prefix: new HarmonyMethod(typeof(ServerOptimizerOverrides),
                    nameof(ServerOptimizerOverrides.Draw_Prefix))
            );

        }
    }
}