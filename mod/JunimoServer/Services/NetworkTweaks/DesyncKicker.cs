using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace JunimoServer.Services.NetworkTweaks
{
    public class DesyncKicker
    {
        private const int DesyncMaxTime = 20;

        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;
        private int _desyncTimer = 0;
        private bool _enableTimer = false;

        public DesyncKicker(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;
            _helper.Events.GameLoop.DayEnding += OnDayEnding;
            _helper.Events.GameLoop.OneSecondUpdateTicked += OnOneSecondTicked;
            _helper.Events.GameLoop.Saving += OnSaving;
        }

        private void OnSaving(object sender, SavingEventArgs e)
        {
            DisableTimer();
            _monitor.Log("Saving");
        }

        private void OnOneSecondTicked(object sender, OneSecondUpdateTickedEventArgs e)
        {
            if (!_enableTimer) return;

            if (_desyncTimer == DesyncMaxTime)
            {
                DisableTimer();

                KickDesynedPlayers();
            }
            else
            {
                _desyncTimer++;
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