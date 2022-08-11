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
        public NetworkTweaker(IModHelper helper, PersistentOptions options)
        {

            _options = options;
            _helper = helper;
            helper.Events.GameLoop.UpdateTicked += OnTick;
        }
        private void OnTick(object sender, UpdateTickedEventArgs e)
        {
            
            if (Game1.netWorldState == null) return;
            
            var multiplayer = _helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();
            var maxPlayers = _options.Data.MaxPlayers;

            if (maxPlayers != multiplayer.playerLimit)
            {
                multiplayer.playerLimit = maxPlayers;
            }

            if (Game1.netWorldState.Value.CurrentPlayerLimit.Value != maxPlayers)
            {
                Game1.netWorldState.Value.CurrentPlayerLimit.Set(maxPlayers);
            }

            if (Game1.netWorldState.Value.HighestPlayerLimit.Value != maxPlayers)
            {
                Game1.netWorldState.Value.HighestPlayerLimit.Set(maxPlayers);
            }
        }
    }
}