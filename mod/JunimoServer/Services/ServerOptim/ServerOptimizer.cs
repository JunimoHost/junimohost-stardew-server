using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace JunimoServer.Services.ServerOptim
{
    public class ServerOptimizer
    {
        private readonly bool _disableRendering;

        public ServerOptimizer(Harmony harmony, IMonitor monitor, IModHelper helper, bool disableRendering)
        {
            _disableRendering = disableRendering;
            ServerOptimizerOverrides.Initialize(monitor);
            harmony.Patch(
                original: AccessTools.Method(typeof(GameRunner), "BeginDraw"),
                prefix: new HarmonyMethod(typeof(ServerOptimizerOverrides),
                    nameof(ServerOptimizerOverrides.Draw_Prefix))
            );

            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.GameLoop.Saving += OnSaving;

        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            if (_disableRendering)
            {
                ServerOptimizerOverrides.DisableDrawing();
            }
        }

        private void OnSaving(object sender, SavingEventArgs e)
        {
            if (_disableRendering)
            {
                ServerOptimizerOverrides.EnableDrawing();
            }
        }

    }
}