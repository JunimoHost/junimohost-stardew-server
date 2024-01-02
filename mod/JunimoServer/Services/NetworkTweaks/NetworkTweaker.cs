using JunimoServer.Services.PersistentOption;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace JunimoServer.Services.NetworkTweaks
{
    public class NetworkTweaker
    {
        private readonly PersistentOptions _options;
        private readonly IModHelper _helper;
        private Multiplayer _multiplayer;

        public NetworkTweaker(IModHelper helper, PersistentOptions options)
        {
            _options = options;
            _helper = helper;
            helper.Events.GameLoop.UpdateTicked += OnTick;
        }

        private void OnTick(object sender, UpdateTickedEventArgs e)
        {
            if (Game1.netWorldState == null || !Game1.hasLoadedGame) return;

            _multiplayer ??= _helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();

            _multiplayer.defaultInterpolationTicks = 6;
            _multiplayer.farmerDeltaBroadcastPeriod = 1;
            _multiplayer.locationDeltaBroadcastPeriod = 1;
            _multiplayer.worldStateDeltaBroadcastPeriod = 1;

            var maxPlayers = _options.Data.MaxPlayers;
            _multiplayer.playerLimit = maxPlayers;

            if (Game1.netWorldState.Value.CurrentPlayerLimit.Value != maxPlayers)
            {
                Game1.netWorldState.Value.CurrentPlayerLimit.Set(maxPlayers);
            }

            if (Game1.netWorldState.Value.HighestPlayerLimit.Value != maxPlayers)
            {
                Game1.netWorldState.Value.HighestPlayerLimit.Set(maxPlayers);
            }

            _multiplayer.UpdateLate(forceSync: true);
        }
    }
}