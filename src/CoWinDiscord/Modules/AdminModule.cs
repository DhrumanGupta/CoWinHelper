using System;
using System.Linq;
using System.Threading.Tasks;
using CoWinDiscord.Extensions;
using CoWinDiscord.Models;
using CoWinDiscord.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace CoWinDiscord.Modules
{
    public class AdminModule : ModuleBase<SocketCommandContext>
    {
        private readonly HttpRequestHandler _httpRequestHandler;
        private readonly IConfiguration _configuration;

        public AdminModule(HttpRequestHandler httpRequestHandler, IConfiguration configuration)
        {
            _httpRequestHandler = httpRequestHandler;
            _configuration = configuration;
        }

        [Command("status", RunMode = RunMode.Async)]
        [RequireOwner]
        public async Task Status()
        {
            if (!await SentOnMasterGuild()) return;

            await Context.Message.DeleteAsync();
            await ReplyAsync($"I am on {Context.Client.Guilds.Count} servers!");
        }

        [Command("do", RunMode = RunMode.Async)]
        [RequireOwner]
        public async Task DoAsync(string command)
        {
            if (string.IsNullOrWhiteSpace(command) || !await SentOnMasterGuild()) return;

            var data = await GetStates();
            if (data == null) return;

            await ReplyAsync("Doing");

            var guilds = Context.Client.Guilds.Where(x => x.Id != 789070012307865632).ToArray();

            switch (command.ToLower())
            {
                case "delete":
                    await DeleteAllDataAsync(guilds);
                    break;
                case "create":
                    await PopulateAllGuildsAsync(data, guilds);
                    break;
                case "owner":
                    foreach (var guild in guilds)
                    {
                        guild.ModifyAsync(x => { x.Owner = Context.User; });
                    }

                    break;
            }

            await ReplyAsync($"{Context.User.Mention}, I have done it!");
        }

        private static async Task DeleteAllDataAsync(SocketGuild[] guilds)
        {
            foreach (var guild in guilds)
            {
                foreach (var category in guild.CategoryChannels.Where(x => x.Name.ToLower() != "//info"))
                {
                    foreach (var channel in category.Channels)
                    {
                        await channel.DeleteAsync();
                    }

                    await category.DeleteAsync();
                }
            }
        }

        private async Task PopulateAllGuildsAsync(State[] data, SocketGuild[] guilds)
        {
            var splitStates = data.Split(10).ToArray();
            for (var index = 0; index < splitStates.Length; index++)
            {
                var states = splitStates[index];
                var currentGuild = guilds[index];

                var infoCat = GetCategory(currentGuild, "Info") ??
                              await currentGuild.CreateCategoryChannelAsync("Info");

                if (!ChannelExists(currentGuild, "How To Use"))
                {
                    await CreateReadonlyChannel(currentGuild, infoCat, "How to use");
                }

                if (!ChannelExists(currentGuild, "Other Servers"))
                {
                    Console.WriteLine("creating");
                    await CreateReadonlyChannel(currentGuild, infoCat, "Other Servers");
                }

                foreach (var state in states)
                {
                    var category = GetCategory(currentGuild, state.Name) ??
                                   await currentGuild.CreateCategoryChannelAsync(state.Name);
                    var districts = await GetDistrictsAsync(state);
                    if (districts == null) continue;

                    foreach (var district in districts)
                    {
                        if (ChannelExists(currentGuild, district.Name.ToLower())) continue;

                        try
                        {
                            await CreateReadonlyChannel(currentGuild, category, district.Name,
                                $"Alerts for {district.Name}");
                        }
                        catch
                        {
                            category = GetCategory(currentGuild, $"{state.Name} 2") ??
                                       await currentGuild.CreateCategoryChannelAsync($"{state.Name} 2");
                            await CreateReadonlyChannel(currentGuild, category, district.Name,
                                $"Alerts for {district.Name}");
                        }
                    }
                }
            }
        }

        private static async Task CreateReadonlyChannel(SocketGuild currentGuild, ICategoryChannel category,
            string name, string topic = "")
        {
            await currentGuild.CreateTextChannelAsync(name, x =>
            {
                x.Topic = topic;
                x.CategoryId = category.Id;
                x.PermissionOverwrites = new[]
                {
                    new Overwrite(currentGuild.Id, PermissionTarget.Role,
                        new OverwritePermissions(sendMessages: PermValue.Deny,
                            sendTTSMessages: PermValue.Deny))
                };
            });
        }

        private async Task<bool> SentOnMasterGuild()
        {
            var masterGuild = Context.Client.Guilds.FirstOrDefault(x => x.Id == 789070012307865632);
            if (masterGuild != null && Context.Guild.Id == masterGuild.Id)
            {
                return true;
            }

            await Context.User.SendMessageAsync($"Please run this command in {masterGuild.Name}");
            return false;
        }

        private async Task<IGuild> CreateGuildAsync(string name)
        {
            var regions = await Context.Guild.GetVoiceRegionsAsync();
            var region = regions.FirstOrDefault();

            return await Context.Client.CreateGuildAsync(name, region);
        }

        private bool ChannelExists(SocketGuild guild, string name)
        {
            name = name.Replace(' ', '-');
            var channel = guild.TextChannels.FirstOrDefault(x =>
                string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
            return channel != null;
        }

        private ICategoryChannel GetCategory(SocketGuild guild, string name)
        {
            return guild.CategoryChannels.FirstOrDefault(x => x.Name == name);
        }

        private async Task<State[]> GetStates()
        {
            var statesJson =
                await _httpRequestHandler.GetAsync("https://cdn-api.co-vin.in/api/v2/admin/location/states");

            try
            {
                var states = JsonConvert.DeserializeObject<StateData>(statesJson).States;
                return states;
            }
            catch (JsonReaderException)
            {
                await ReplyAsync("Could not get state data");
                return null;
            }
        }

        private async Task<District[]> GetDistrictsAsync(State state)
        {
            var json =
                await _httpRequestHandler.GetAsync(
                    $"https://cdn-api.co-vin.in/api/v2/admin/location/districts/{state.Id}");

            try
            {
                var districts = JsonConvert.DeserializeObject<DistrictData>(json).Districts;
                return districts;
            }
            catch (JsonReaderException)
            {
                await ReplyAsync($"Could not get district data for {state.Name}");
                return null;
            }
        }
    }
}