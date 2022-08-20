using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Network;

namespace JunimoServer.Services.NetworkTweaks
{
    public class DesyncKicker
    {
        private const int DesyncMaxTime = 20;

        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;
        private int _desyncTimer = 0;
        private bool _enableTimer = false;

        public DesyncKicker(IModHelper helper, IMonitor monitor, Harmony harmony)
        {
            _helper = helper;
            _monitor = monitor;
            _helper.Events.GameLoop.DayEnding += OnDayEnding;
            // _helper.Events.GameLoop.OneSecondUpdateTicked += OnOneSecondTicked;
            _helper.Events.GameLoop.Saving += OnSaving;

            // DesyncKickerOverrides.Initialize(monitor, helper);
            //
            // harmony.Patch(
            //     original: AccessTools.Method(typeof(NewDaySynchronizer), "processMessages"),
            //     prefix: new HarmonyMethod(typeof(DesyncKickerOverrides),
            //         nameof(DesyncKickerOverrides.NewDaySynchronizer_processMessages_prefix))
            // );
        }

        private void OnSaving(object sender, SavingEventArgs e)
        {
            LogChecks();
            DisableTimer();
            _monitor.Log("Saving");
        }

        private void OnOneSecondTicked(object sender, OneSecondUpdateTickedEventArgs e)
        {
            if (!_enableTimer) return;

            LogChecks();
            if (_desyncTimer == DesyncMaxTime)
            {
                DisableTimer();

                // KickDesynedPlayers();
            }
            else
            {
                _desyncTimer++;
            }
        }

        private void LogChecks()
        {
            _monitor.Log("-----------------------------------------------------");

            var checks = new string[]
            {
                "ready_for_save",
                "wakeup"
            };

            foreach (var farmer in Game1.getOnlineFarmers())
            {
                foreach (var check in checks)
                {
                    var passedCheck = Game1.player.team.IsOtherFarmerReady(check, farmer);
                    _monitor.Log($"{farmer.Name}:{farmer.UniqueMultiplayerID} {check}:{passedCheck}");
                }

                _monitor.Log(
                    $"{farmer.Name}:{farmer.UniqueMultiplayerID} endOfNightStatus: {Game1.player.team.endOfNightStatus.GetStatusText(farmer.UniqueMultiplayerID)}");
            }
        }

        private void DisableTimer()
        {
            _enableTimer = false;
            _desyncTimer = 0;
        }

        private void OnDayEnding(object sender, DayEndingEventArgs e)
        {
            _monitor.Log("DayEnding");

            _enableTimer = true;
            Task.Run(async () =>
            {
                _monitor.Log("waiting 15 sec to kick");

                await Task.Delay(15 * 1000);
                _monitor.Log("waited 15 sec to kick");

                if (_enableTimer)
                {
                    _monitor.Log("still stuck");

                    LogChecks();
                    var readyPlayers = _helper.Reflection.GetMethod(Game1.newDaySync, "barrierPlayers")
                        .Invoke<HashSet<long>>("sleep");
                    foreach (var key in (IEnumerable<long>)Game1.otherFarmers.Keys)
                    {
                        if (!readyPlayers.Contains(key))
                        {
                            Game1.server.kick(key);
                            _monitor.Log("kicking: " + key);
                        }
                    }
                }
            });
        }

        private void KickDesynedPlayers()
        {
            foreach (var farmer in Game1.getOnlineFarmers()
                         .Where(farmer => !Game1.player.team.IsOtherFarmerReady("ready_for_save", farmer)))
            {
                _monitor.Log($"Kicking {farmer.Name} because they aren't ready_for_save");
                Game1.server.kick(farmer.UniqueMultiplayerID);
            }
        }
    }
}