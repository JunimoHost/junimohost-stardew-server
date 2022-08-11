using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using JunimoServer.Services.GameCreator;
using StardewModdingAPI;

namespace JunimoServer.Services.SteamAuth
{

    public class SteamTicket
    {
        public byte[] Ticket { get; set; }
        public uint TicketSize { get; set; }
        public string Name { get; set; }

        public SteamTicket(byte[] ticket, uint ticketSize, string name)
        {
            Ticket = ticket;
            TicketSize = ticketSize;
            Name = name;
        }
    }

    public class SteamAuthClient
    {
        private readonly HttpClient _httpClient;
        private readonly IMonitor _monitor;
        public SteamAuthClient(HttpClient httpClient, IMonitor monitor)
        {
            _monitor = monitor;
            _httpClient = httpClient;
        }


        public async Task<SteamTicket> GetTicket()
        {
            _monitor.Log("Fetching ticket from steam auth service", LogLevel.Info);

            var response = await _httpClient.GetAsync("/steam-ticket");
            var ticket = await response.Content.ReadFromJsonAsync<SteamTicket>();

            _monitor.Log($"Received config: {ticket}", LogLevel.Info);
            return ticket;
        }

        public SteamTicket GetTicketSync()
        {
            try
            {
                var task = GetTicket();
                task.Wait();
                return task.Result;
            }
            catch (Exception e)
            {
                _monitor.Log(e.ToString(), LogLevel.Error);
                Environment.Exit(1);
                return null;
            }
        }
    }
}