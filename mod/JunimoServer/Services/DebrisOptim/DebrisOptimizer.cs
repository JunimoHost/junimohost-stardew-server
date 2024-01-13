using System;
using System.Diagnostics;
using HarmonyLib;
using Microsoft.VisualBasic;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.SDKs;

namespace JunimoServer.Services.DebrisOptim
{
    public class DebrisOptimizer
    {
        private readonly IMonitor _monitor;

        public DebrisOptimizer(
            Harmony harmony,
            IMonitor monitor,
            IModHelper helper
        )
        {
            _monitor = monitor;
            DebrisOptimizerOverrides.Initialize(monitor, helper);
            harmony.Patch(
                original: AccessTools.Method(typeof(Debris), nameof(Debris.updateChunks)),
                prefix: new HarmonyMethod(typeof(DebrisOptimizerOverrides),
                    nameof(DebrisOptimizerOverrides.updateChunks_Prefix))
            );
            
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            DebrisOptimizerOverrides.Update();
        }
    }
}