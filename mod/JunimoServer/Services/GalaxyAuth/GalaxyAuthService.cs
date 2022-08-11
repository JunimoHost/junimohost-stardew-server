using HarmonyLib;
using JunimoServer.Services.SteamAuth;
using StardewModdingAPI;
using StardewValley.SDKs;

namespace JunimoServer.Services.GalaxyAuth
{
    public class GalaxyAuthService
    {
        public GalaxyAuthService(IMonitor monitor, IModHelper helper, Harmony harmony, SteamAuthClient steamAuthClient)
        {
            GalaxyAuthOverrides.Initialize(monitor, helper, steamAuthClient);
            // harmony.Patch(
            //     original: AccessTools.Method(typeof(SteamHelper), nameof(SteamHelper.Update)),
            //     prefix: new HarmonyMethod(typeof(GalaxyAuthOverrides),
            //         nameof(GalaxyAuthOverrides.SteamHelperUpdate_Prefix)
            //     ));
            harmony.Patch(
                original: AccessTools.Method(typeof(SteamHelper), nameof(SteamHelper.Initialize)),
                prefix: new HarmonyMethod(typeof(GalaxyAuthOverrides),
                    nameof(GalaxyAuthOverrides.SteamHelperInitialize_Prefix)
                ));
        }
    }
}