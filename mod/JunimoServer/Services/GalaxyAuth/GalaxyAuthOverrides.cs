using System;
using Galaxy.Api;
using Junimohost.Stardewsteamauth.V1;
using StardewModdingAPI;
using StardewValley.SDKs;
using Steamworks;

namespace JunimoServer.Services.GalaxyAuth
{
    public class GalaxyAuthOverrides
    {
        private static IMonitor _monitor;
        private static IModHelper _helper;
        private static StardewSteamAuthService.StardewSteamAuthServiceClient _steamAuthClient;

        private static GalaxyHelper.OperationalStateChangeListener _stateChangeListener;
        private static GalaxyHelper.AuthListener _authListener;


        public static void Initialize(IMonitor monitor, IModHelper helper, StardewSteamAuthService.StardewSteamAuthServiceClient steamAuthClient)
        {
            _monitor = monitor;
            _helper = helper;
            _steamAuthClient = steamAuthClient;
        }
        

        public static bool SteamHelperInitialize_Prefix(SteamHelper __instance)
        {
            var onGalaxyAuthSuccess =
                new Action(() => _helper.Reflection.GetMethod(__instance, "onGalaxyAuthSuccess").Invoke());
            var onGalaxyAuthFailure =
                new Action<IAuthListener.FailureReason>((reason) =>
                    _helper.Reflection.GetMethod(__instance, "onGalaxyAuthFailure").Invoke(reason));
            var onGalaxyAuthLost =
                new Action(() => _helper.Reflection.GetMethod(__instance, "onGalaxyAuthLost").Invoke());

            var onGalaxyStateChange =
                new Action<uint>((num) => _helper.Reflection.GetMethod(__instance, "onGalaxyStateChange").Invoke(num));

            try
            {
                __instance.active = SteamAPI.Init();
                Console.WriteLine("Steam logged on: " + SteamUser.BLoggedOn().ToString());
                if (__instance.active)
                {
                    _helper.Reflection.GetField<bool>(__instance, "_runningOnSteamDeck").SetValue(false);
                    Console.WriteLine("Initializing GalaxySDK");
                    GalaxyInstance.Init(new InitParams("48767653913349277",
                        "58be5c2e55d7f535cf8c4b6bbc09d185de90b152c8c42703cc13502465f0d04a", "."));

                    _authListener =
                        new GalaxyHelper.AuthListener(onGalaxyAuthSuccess, onGalaxyAuthFailure, onGalaxyAuthLost);
                    _stateChangeListener = new GalaxyHelper.OperationalStateChangeListener(onGalaxyStateChange);

                    Console.WriteLine("Requesting Steam app ticket");

                    var ticket = _steamAuthClient.GetSteamTicket(new GetSteamTicketRequest());
                    GalaxyInstance.User().SignInSteam(ticket.Ticket.ToByteArray(), Convert.ToUInt32(ticket.TicketLength), ticket.Name);


                    _helper.Reflection.GetProperty<int>(__instance, "ConnectionProgress")
                        .SetValue(__instance.ConnectionProgress + 1);
                }
            }
            catch (Exception value)
            {
                Console.WriteLine(value);
                __instance.active = false;
                _helper.Reflection.GetProperty<bool>(__instance, "ConnectionFinished").SetValue(true);
            }

            return false;
        }

    }
}