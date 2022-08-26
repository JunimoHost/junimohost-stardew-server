using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HarmonyLib;
using JunimoServer.Util;
using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Network;

namespace JunimoServer.Services.NetworkTweaks
{
    public class DesyncKicker
    {
        private const int DesyncMaxTime = 20;

        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;
        private bool _inNewDayBarrier = false;
        private bool _isSaving = false;

        public DesyncKicker(IModHelper helper, IMonitor monitor, Harmony harmony)
        {
            _helper = helper;
            _monitor = monitor;
            _helper.Events.GameLoop.DayEnding += OnDayEnding;
            // _helper.Events.GameLoop.OneSecondUpdateTicked += OnOneSecondTicked;
            _helper.Events.GameLoop.Saving += OnSaving;
            _helper.Events.GameLoop.Saved += OnSaved;
            helper.Events.Specialized.UnvalidatedUpdateTicked += OnUnvalidatedTicked;

            // DesyncKickerOverrides.Initialize(monitor, helper);
            //
            // harmony.Patch(
            //     original: AccessTools.Method(typeof(NewDaySynchronizer), "processMessages"),
            //     prefix: new HarmonyMethod(typeof(DesyncKickerOverrides),
            //         nameof(DesyncKickerOverrides.NewDaySynchronizer_processMessages_prefix))
            // );
        }
        private void OnUnvalidatedTicked(object sender, UnvalidatedUpdateTickedEventArgs e)
        {
            if (_inNewDayBarrier || _isSaving)
            {
                LogChecks();
            }
        }

        private void OnSaved(object sender, SavedEventArgs e)
        {

            _isSaving = false;
            _monitor.Log("Saved");
            LogChecks();
        }

        private void OnSaving(object sender, SavingEventArgs e)
        {
            LogChecks();
            _inNewDayBarrier = false;
            _monitor.Log("Saving");
            _isSaving = true;

            Task.Run(async () =>
            {
                _monitor.Log("waiting 45 sec to kick shipping + waiting for others");

                await Task.Delay(45 * 1000);
                _monitor.Log("waited 45 sec to kick shipping + waiting for others");
                LogChecks();
                if (_isSaving)
                {
                    KickDesyncedPlayers();
                }
            });
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


        private void OnDayEnding(object sender, DayEndingEventArgs e)
        {
            if (SDate.Now().IsDayZero()) return;
            _monitor.Log("DayEnding");

            _inNewDayBarrier = true;
            Task.Run(async () =>
            {
                _monitor.Log("waiting 20 sec to kick barrier");

                await Task.Delay(20 * 1000);
                _monitor.Log("waited 20 sec to kick barrier");

                if (_inNewDayBarrier)
                {
                    _monitor.Log("still stuck in barrier, going to try kicking");

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

        private void KickDesyncedPlayers()
        {
            foreach (var farmer in Game1.otherFarmers.Values.ToArray()
                         .Where(farmer => !Game1.player.team.IsOtherFarmerReady("ready_for_save", farmer)))
            {
                _monitor.Log($"Kicking {farmer.Name} because they aren't ready_for_save");
                Game1.server.kick(farmer.UniqueMultiplayerID);
            }
        }
    }
}