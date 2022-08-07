using StardewModdingAPI.Utilities;

namespace JunimoServer.Util
{
    public static class SDateExtensions
    {
        public static bool EqualsIgnoreYear(this SDate date, SDate otherDate)
        {
            return date.Day == otherDate.Day && date.Season == otherDate.Season;
        }
    }
}