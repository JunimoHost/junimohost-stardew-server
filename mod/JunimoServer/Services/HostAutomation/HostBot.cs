using System.Collections.Generic;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace JunimoServer.Services.HostAutomation
{
    public class HostBot
    {

        private readonly ActivityList activities;
        public HostBot(IModHelper helper, IMonitor monitor)
        {

            activities = new ActivityList
            {
                new HideHostActivity(),
            };
            
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.UpdateTicked += OnTick;
            helper.Events.GameLoop.DayStarted += OnDayStarted;
        }
        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            activities.DayStartAll();
        }
        private void OnTick(object sender, UpdateTickedEventArgs e)
        {
            activities.TickAll();
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
          activities.EnableAll();
        }
    }
}