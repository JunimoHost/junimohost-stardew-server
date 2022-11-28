using System;
using System.Diagnostics;
using HarmonyLib;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace JunimoServer.Services.ServerOptim
{
    public class ServerOptimizer
    {
        private readonly bool _disableRendering;
        private readonly IMonitor _monitor;

        public ServerOptimizer(
            Harmony harmony,
            IMonitor monitor,
            IModHelper helper,
            bool disableRendering,
            bool enableModIncompatibleOptimizations
        )
        {
            _monitor = monitor;
            _disableRendering = disableRendering;
            ServerOptimizerOverrides.Initialize(monitor);
            harmony.Patch(
                original: AccessTools.Method(typeof(GameRunner), "BeginDraw"),
                prefix: new HarmonyMethod(typeof(ServerOptimizerOverrides),
                    nameof(ServerOptimizerOverrides.Draw_Prefix))
            );


        

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

            if (disableRendering)
            {
                harmony.Patch(
                    original: AccessTools.Method("StardewModdingAPI.Framework.SCore:OnInstanceContentLoaded"),
                    prefix: new HarmonyMethod(typeof(ServerOptimizerOverrides),
                        nameof(ServerOptimizerOverrides.AssignNullDisplay_Prefix)));

                harmony.Patch(
                    original: AccessTools.Method("StardewModdingAPI.Framework.SCore:GetMapDisplayDevice"),
                    prefix: new HarmonyMethod(typeof(ServerOptimizerOverrides),
                        nameof(ServerOptimizerOverrides.ReturnNullDisplay_Prefix)));
                
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
            }



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
            helper.Events.GameLoop.DayEnding += OnDayEnding;

        }

        private void OnDayEnding(object sender, DayEndingEventArgs e)
        {
            var before = checked ((long) Math.Round(Process.GetCurrentProcess().PrivateMemorySize64 / 1024.0 / 1024.0));
            _monitor.Log($"Running GC", LogLevel.Info);
            GC.Collect(generation: 0, GCCollectionMode.Forced, blocking: true);
            GC.Collect(generation: 1, GCCollectionMode.Forced, blocking: true);
            GC.Collect(generation: 2, GCCollectionMode.Forced, blocking: true);
            var after = checked ((long) Math.Round(Process.GetCurrentProcess().PrivateMemorySize64 / 1024.0 / 1024.0));
            var beforeFormated = Strings.Format(before / 1024.0, "0.00") + " GB";
            var afterFormated = Strings.Format(after / 1024.0, "0.00") + " GB";
            _monitor.Log($"Ran GC {beforeFormated} -> {afterFormated}", LogLevel.Info);
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