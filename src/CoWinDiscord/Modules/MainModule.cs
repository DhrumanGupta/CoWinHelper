using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoWinDiscord.Models;
using CoWinDiscord.Services;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace CoWinDiscord.Modules
{
    public class MainModule
    {
        private readonly HttpRequestHandler _httpRequestHandler;
        private readonly DiscordSocketClient _client;

        public MainModule(DiscordSocketClient client)
        {
            _httpRequestHandler = new HttpRequestHandler();
            _client = client;
        }

        public async Task StartLoop()
        {
            Console.WriteLine("\nStarted Main Loop\n");
            while (true)
            {
                var server = _client.Guilds.FirstOrDefault(x => x.Id != 789070012307865632);
                await ProcessData(9, server);

                await Task.Delay(15000);
            }
        }

        private async Task ProcessData(int stateId, SocketGuild server)
        {
            var districts = await GetDistrictsAsync(stateId);
            if (districts == null)
            {
                Console.WriteLine("shit fucked ip");
                return;
            }

            foreach (var district in districts)
            {
                await ManageChannelData(server, district);
                await Task.Delay(700);
            }
            
            Console.WriteLine($"Checked at {DateTime.Now}");
        }

        private async Task ManageChannelData(SocketGuild server, District district)
        {
            var centers = await GetCentersAsync(district);
            if (centers is not {Length: > 0})
            {
#if DEBUG
                Console.WriteLine("shit fucked ip");
#endif
                return;
            }

            var possibleCenters = new List<Center>();
            foreach (var center in centers)
            {
                center.sessions = center.sessions
                    .Where(session => session.available_capacity > 0 && session.min_age_limit < 45)
                    .ToArray();

                if (center.sessions.Length == 0) continue;
                possibleCenters.Add(center);
            }

            if (possibleCenters.Count <= 0) return;
#if DEBUG
            Console.WriteLine($"Found some centers for {district.Name}");
#endif

            var embeds = possibleCenters
                .Select(EmbedsFromCenter)
                .ToArray();

            var channel = GetChannel(server, district.Name);
            var tasks = (from centerEmbeds in embeds
                from embed in centerEmbeds
                select channel.SendMessageAsync(null, false, embed)).Cast<Task>().ToArray();

            await Task.WhenAll(tasks);
        }

        private SocketTextChannel GetChannel(SocketGuild guild, string name)
        {
            name = name.Replace(' ', '-');
            var channel = guild.TextChannels.FirstOrDefault(x =>
                string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
            return channel;
        }

        private Embed[] EmbedsFromCenter(Center center)
        {
            var embeds = new Embed[center.sessions.Length];

            for (var i = 0; i < embeds.Length; i++)
            {
                var session = center.sessions[i];
                embeds[i] = new EmbedBuilder()
                    .WithColor(Color.Green)
                    .WithTitle($"{session.vaccine} available")
                    .AddField("Address", $"{center.name},\n{center.district_name}")
                    .AddField("Date", session.date)
                    .AddField("Available Capacity", session.available_capacity, true)
                    // .WithFooter("To donate, run !donate in #bot-commands")
                    .WithTimestamp(DateTimeOffset.Now)
                    .Build();
            }

            return embeds;
        }

        private async Task<District[]> GetDistrictsAsync(int stateId)
        {
            var json =
                await _httpRequestHandler.GetAsync(
                    $"https://cdn-api.co-vin.in/api/v2/admin/location/districts/{stateId}");

            try
            {
                var districts = JsonConvert.DeserializeObject<DistrictData>(json)?.Districts;
                return districts;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private async Task<Center[]> GetCentersAsync(District district)
        {
            var json =
                await _httpRequestHandler.GetAsync(
                    $"https://cdn-api.co-vin.in/api/v2/appointment/sessions/calendarByDistrict?district_id={district.Id}&date={DateTime.Today:dd/MM/yyyy}");

            try
            {
                var centerData = JsonConvert.DeserializeObject<CenterData>(json)?.centers;
                return centerData;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}