using HarmonyLib;
using StardewModdingAPI;
using StardewValley;

namespace JunimoServer.Services.ServerOptim
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

        public void DisableDrawing()
        {
            ServerOptimizerOverrides.DisableDrawing();
        }
        
        public void EnableDrawing()
        {
            ServerOptimizerOverrides.EnableDrawing();
        }
    }
}