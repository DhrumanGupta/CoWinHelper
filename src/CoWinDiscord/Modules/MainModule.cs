using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoWinDiscord.Models;
using CoWinDiscord.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace CoWinDiscord.Modules
{
    public class MainModule
    {
        private readonly HttpRequestHandler _httpRequestHandler;
        private readonly DiscordSocketClient _client;

        public MainModule(HttpRequestHandler httpRequestHandler, DiscordSocketClient client)
        {
            _httpRequestHandler = httpRequestHandler;
            _client = client;
        }
        
        public async Task StartLoop()
        {
            Console.WriteLine("start");
            while (true)
            {
                var states = await GetStatesAsync();
                if (states == null) continue;

                var servers = _client.Guilds.Where(x => x.Id != 789070012307865632).ToArray();
                Console.WriteLine($"\n\n\n\n{servers.Length}\n\n\n\n");

                for (var index = 0; index < states.Length; index++)
                {
                    var state = states[index];
                    var districts = await GetDistrictsAsync(state);
                    if (districts == null) continue;

                    var server = servers[index/10];

                    foreach (var district in districts)
                    {
                        var centers = await GetCentersAsync(district);
                        if (centers is not {Length: > 0}) continue;

                        var possibleCenters = new List<Center>();
                        foreach (var center in centers)
                        {
                            var cent = center;
                            var sessions = new List<Session>();
                            foreach (var session in center.sessions.Where(session =>
                                session.available_capacity > 0 && session.min_age_limit < 45))
                            {
                                sessions.Add(session);
                            }

                            cent.sessions = sessions.ToArray();
                            possibleCenters.Add(cent);
                        }
                        
                        if (possibleCenters.Count <= 0) continue;

                        var embeds = possibleCenters
                            .Select(EmbedsFromCenter)
                            .ToArray();
                        
                        var channel = GetChannel(server, district.Name);
                        foreach (var centerEmbeds in embeds)
                        {
                            foreach (var embed in centerEmbeds)
                            {
                                await channel.SendMessageAsync(null, false, embed);
                            }
                        }
                    }
                }

                await Task.Delay(5000);
            }
        }
        
        private SocketTextChannel GetChannel(SocketGuild guild, string name)
        {
            name = name.Replace(' ', '-');
            var channel = guild.TextChannels.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
            return channel;
        }

        private Embed[] EmbedsFromCenter(Center center)
        {
            var embeds = new Embed[center.sessions.Length];

            for (var i = 0; i < embeds.Length; i++)
            {
                embeds[i] = new EmbedBuilder()
                    .WithColor(Color.Green)
                    .WithTitle($"{center.sessions[i].vaccine} available")
                    .AddField("Center Name", center.name)
                    .WithFooter($"{center.sessions[i].available_capacity} seats available!")
                    .WithTimestamp(DateTimeOffset.Now)
                    .Build();
            }

            return embeds;
        }
        
        private async Task<State[]> GetStatesAsync()
        {
            var statesJson =
                await _httpRequestHandler.GetAsync("https://cdn-api.co-vin.in/api/v2/admin/location/states");

            try
            {
                var states = JsonConvert.DeserializeObject<StateData>(statesJson).States;
                return states;
            }
            catch (Exception)
            {
                return null;
            }
        }
        
        private async Task<District[]> GetDistrictsAsync(State state)
        {
            var json =
                await _httpRequestHandler.GetAsync($"https://cdn-api.co-vin.in/api/v2/admin/location/districts/{state.Id}");

            try
            {
                var districts = JsonConvert.DeserializeObject<DistrictData>(json).Districts;
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
                var centerData = JsonConvert.DeserializeObject<CenterData>(json).centers;
                return centerData;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}