using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using JunimoServer.Util;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace JunimoServer.Services.NetworkTweaks
{
    public class DesyncKicker
    {
        private const int barrierDesyncMaxTime = 20;
        private const int endOfDayDesyncMaxTime = 60;

        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;
        private bool _inNewDayBarrier = false;
        private bool _isSaving = false;

        public DesyncKicker(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;
            _helper.Events.GameLoop.DayEnding += OnDayEnding;
            _helper.Events.GameLoop.Saving += OnSaving;
            _helper.Events.GameLoop.Saved += OnSaved;

        }

        private void OnSaved(object sender, SavedEventArgs e)
        {

            _isSaving = false;
        }

        private void OnSaving(object sender, SavingEventArgs e)
        {
            _inNewDayBarrier = false;
            _monitor.Log("Saving");
            _isSaving = true;

            Task.Run(async () =>
            {
                _monitor.Log($"waiting {endOfDayDesyncMaxTime} sec to kick non-ready players");

                await Task.Delay(endOfDayDesyncMaxTime * 1000);
                _monitor.Log($"waited {endOfDayDesyncMaxTime} sec to kick non-ready players");
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
                _monitor.Log($"waiting {barrierDesyncMaxTime} sec to kick barrier");

                await Task.Delay(barrierDesyncMaxTime * 1000);
                _monitor.Log($"waited {barrierDesyncMaxTime} sec to kick barrier");

                if (_inNewDayBarrier)
                {
                    _monitor.Log("still stuck in barrier, going to try kicking");

                    var readyPlayers = _helper.Reflection.GetMethod(Game1.newDaySync, "barrierPlayers")
                        .Invoke<HashSet<long>>("sleep");
                    foreach (var key in (IEnumerable<long>)Game1.otherFarmers.Keys)
                    {
                        if (!readyPlayers.Contains(key))
                        {
                            Game1.server.kick(key);
                            _monitor.Log("kicking due to not making past barrier: " + key);
                        }
                    }
                }
            });
        }

        private void KickDesyncedPlayers()
        {
            foreach (var farmer in Game1.otherFarmers.Values.ToArray()
                         .Where(farmer => Game1.player.team.endOfNightStatus.GetStatusText(farmer.UniqueMultiplayerID) != "ready"))
            {
                _monitor.Log($"Kicking {farmer.Name} because they aren't ready");
                Game1.server.kick(farmer.UniqueMultiplayerID);
            }
        }
    }
}