using System;
using Galaxy.Api;
using JunimoServer.Services.SteamAuth;
using StardewModdingAPI;
using StardewValley.SDKs;
using Steamworks;

namespace JunimoServer.Services.GalaxyAuth
{
    public class GalaxyAuthOverrides
    {
        private static IMonitor _monitor;
        private static IModHelper _helper;
        private static SteamAuthClient _steamAuthClient;

        private static GalaxyHelper.OperationalStateChangeListener _stateChangeListener;
        private static GalaxyHelper.AuthListener _authListener;
        private const string SaveKey = "JunimoHost.steamticket";


        public static void Initialize(IMonitor monitor, IModHelper helper, SteamAuthClient steamAuthClient)
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

                    var ticket = _steamAuthClient.GetTicketSync();
                    GalaxyInstance.User().SignInSteam(ticket.Ticket, ticket.TicketSize, ticket.Name);


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

        public class SteamTicket
        {
            public byte[] Ticket;
            public uint TicketSize;
            public string Name;

            public SteamTicket(byte[] ticket, uint ticketSize, string name)
            {
                Ticket = ticket;
                TicketSize = ticketSize;
                Name = name;
            }
        }

        const bool Write = true;
        private static void OnEncryptedAppTicketResponse(SteamHelper instance, EncryptedAppTicketResponse_t response,
            bool ioFailure)
        {
            if (response.m_eResult == EResult.k_EResultOK)
            {
                if (Write)
                {
                    byte[] ticket = new byte[1024];
                    uint ticketSize;
                    SteamUser.GetEncryptedAppTicket(ticket, 1024, out ticketSize);


                    var saveTicket = new SteamTicket(ticket, ticketSize, SteamFriends.GetPersonaName());
                    _helper.Data.WriteGlobalData(SaveKey, saveTicket);
                    Environment.Exit(0);

                    Console.WriteLine("Signing into GalaxySDK");
                    GalaxyInstance.User().SignInSteam(ticket, ticketSize, SteamFriends.GetPersonaName());
                }
                else
                {
                    var ticket = _helper.Data.ReadGlobalData<SteamTicket>(SaveKey);
                    
                    Console.WriteLine("Signing into GalaxySDK");
                }


             

                _helper.Reflection.GetProperty<int>(instance, "ConnectionProgress")
                    .SetValue(instance.ConnectionProgress + 1);
                return;
            }

            Console.WriteLine("Failed to retrieve encrypted app ticket: " + response.m_eResult.ToString() + ", " +
                              ioFailure.ToString());
            _helper.Reflection.GetProperty<bool>(instance, "ConnectionFinished").SetValue(true);
        }
    }
}