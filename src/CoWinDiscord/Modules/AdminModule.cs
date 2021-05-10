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

        public AdminModule(IConfiguration configuration)
        {
            _httpRequestHandler = new HttpRequestHandler();
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

            await ReplyAsync("Doing");

            var guild = Context.Client.Guilds.FirstOrDefault(x => x.Id != 789070012307865632);

            switch (command.ToLower())
            {
                case "delete":
                    await DeleteAllDataAsync(guild);
                    break;
                case "create":
                    await PopulateAllGuildsAsync(new State {Id = 9, Name = "Delhi"}, guild);
                    break;
            }

            await ReplyAsync($"{Context.User.Mention}, I have done it!");
        }

        private static async Task DeleteAllDataAsync(SocketGuild guild)
        {
            var channelIds = guild.CategoryChannels
                .Where(x => x.Name.ToLower() != "info" || x.Name.ToLower() != "other")
                .Select(x => x.Channels)
                .Select(x => x.Select(x => x.Id));
            
            Console.WriteLine(channelIds);
        }

        private async Task PopulateAllGuildsAsync(State state, SocketGuild guild)
        {
            var infoCat = GetCategory(guild, "Info") ??
                          await guild.CreateCategoryChannelAsync("Info");

            if (!ChannelExists(guild, "How To Use"))
            {
                await CreateReadonlyChannel(guild, infoCat, "How to use");
            }

            var supportCat = GetCategory(guild, "Other") ??
                             await guild.CreateCategoryChannelAsync("Other");

            if (!ChannelExists(guild, "Feedback"))
            {
                await guild.CreateTextChannelAsync("Feedback", x =>
                {
                    x.CategoryId = supportCat.Id;
                });
            }
            
            if (!ChannelExists(guild, "commands"))
            {
                await guild.CreateTextChannelAsync("commands", x =>
                {
                    x.CategoryId = supportCat.Id;
                });
            }

            var category = GetCategory(guild, state.Name) ??
                           await guild.CreateCategoryChannelAsync(state.Name);
            var districts = await GetDistrictsAsync(state);
            if (districts == null) return;

            foreach (var district in districts)
            {
                if (ChannelExists(guild, district.Name.ToLower())) continue;

                try
                {
                    await CreateReadonlyChannel(guild, category, district.Name,
                        $"Alerts for {district.Name}");
                }
                catch
                {
                    category = GetCategory(guild, $"{state.Name} 2") ??
                               await guild.CreateCategoryChannelAsync($"{state.Name} 2");
                    await CreateReadonlyChannel(guild, category, district.Name,
                        $"Alerts for {district.Name}");
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