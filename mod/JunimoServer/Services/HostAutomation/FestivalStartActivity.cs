using System.Collections.Generic;
using StardewValley;

namespace JunimoServer.Services.HostAutomation
{
    public class FestivalStartActivity: Activity
    {

        protected override void OnTick(int tickNum)
        {
            Utility.isFestivalDay(1,"Fall");

            var data = Game1.temporaryContent.Load<object>("Data\\Festivals");
            Utility.isFestivalDay(1,"Fall");

        }
    }
}