using System;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace JunimoServer.Services.ServerOptim
{
    public class ServerOptimizer
    {
        private readonly bool _disableRendering;

        public ServerOptimizer(
            Harmony harmony,
            IMonitor monitor,
            IModHelper helper,
            bool disableRendering,
            bool enableModIncompatibleOptimizations
        )
        {
            _disableRendering = disableRendering;
            ServerOptimizerOverrides.Initialize(monitor);
            harmony.Patch(
                original: AccessTools.Method(typeof(GameRunner), "BeginDraw"),
                prefix: new HarmonyMethod(typeof(ServerOptimizerOverrides),
                    nameof(ServerOptimizerOverrides.Draw_Prefix))
            );


            harmony.Patch(
                original: AccessTools.Method("Microsoft.Xna.Framework.Input.Keyboard:PlatformGetState"),
                prefix: new HarmonyMethod(typeof(ServerOptimizerOverrides),
                    nameof(ServerOptimizerOverrides.Disable_Prefix)));

            harmony.Patch(
                original: AccessTools.Method("Microsoft.Xna.Framework.Input.Mouse:PlatformGetState"),
                prefix: new HarmonyMethod(typeof(ServerOptimizerOverrides),
                    nameof(ServerOptimizerOverrides.Disable_Prefix)));

            harmony.Patch(
                original: AccessTools.Method("StardewValley.Game1:UpdateControlInput"),
                prefix: new HarmonyMethod(typeof(ServerOptimizerOverrides),
                    nameof(ServerOptimizerOverrides.Disable_Prefix)));

            harmony.Patch(
                original: AccessTools.Method("StardewValley.Game1:updateMusic"),
                prefix: new HarmonyMethod(typeof(ServerOptimizerOverrides),
                    nameof(ServerOptimizerOverrides.Disable_Prefix)));

            harmony.Patch(
                original: AccessTools.Method("StardewValley.BellsAndWhistles.Butterfly:update"),
                prefix: new HarmonyMethod(typeof(ServerOptimizerOverrides),
                    nameof(ServerOptimizerOverrides.Disable_Prefix)));

            harmony.Patch(
                original: AccessTools.Method("StardewValley.BellsAndWhistles.AmbientLocationSounds:update"),
                prefix: new HarmonyMethod(typeof(ServerOptimizerOverrides),
                    nameof(ServerOptimizerOverrides.Disable_Prefix)));


            if (enableModIncompatibleOptimizations)
            {
                harmony.Patch(
                    original: AccessTools.Method("StardewModdingAPI.Framework.StateTracking.Snapshots.PlayerSnapshot:Update"),
                    prefix: new HarmonyMethod(typeof(ServerOptimizerOverrides),
                        nameof(ServerOptimizerOverrides.Disable_Prefix)));


                harmony.Patch(
                    original: AccessTools.Method("StardewModdingAPI.Framework.StateTracking.Snapshots.WorldLocationsSnapshot:Update"),
                    prefix: new HarmonyMethod(typeof(ServerOptimizerOverrides),
                        nameof(ServerOptimizerOverrides.Disable_Prefix)));

                harmony.Patch(
                    original: AccessTools.Method("StardewModdingAPI.Framework.StateTracking.PlayerTracker:Update"),
                    prefix: new HarmonyMethod(typeof(ServerOptimizerOverrides),
                        nameof(ServerOptimizerOverrides.Disable_Prefix)));

                harmony.Patch(
                    original: AccessTools.Method("StardewModdingAPI.Framework.StateTracking.WorldLocationsTracker:Update"),
                    prefix: new HarmonyMethod(typeof(ServerOptimizerOverrides),
                        nameof(ServerOptimizerOverrides.Disable_Prefix)));

                harmony.Patch(
                    original: AccessTools.Method("StardewModdingAPI.Framework.StateTracking.WorldLocationsTracker:Reset"),
                    prefix: new HarmonyMethod(typeof(ServerOptimizerOverrides),
                        nameof(ServerOptimizerOverrides.Disable_Prefix)));
            }

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