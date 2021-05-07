using System;
using System.Linq;
using System.Threading.Tasks;
using CoWinDiscord.Models;
using CoWinDiscord.Services;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using static System.String;

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

        [Command("status")]
        [RequireOwner]
        public async Task Status()
        {
            if (!await SentOnMasterGuild()) return;
            await ReplyAsync($"I am on {Context.Client.Guilds.Count} servers!");
        }

        [Command("create", RunMode = RunMode.Async)]
        [RequireOwner]
        public async Task CreateAsync()
        {
            if (!await SentOnMasterGuild() || !await IsAdmin()) return;

            var states = await GetStates();
            if (states == null) return;

            await ReplyAsync("Creating channels");

            foreach (var state in states)
            {
                var category = GetCategory(state.Name) ??
                               (IGuildChannel) await Context.Guild.CreateCategoryChannelAsync(state.Name);

                var districts = await GetDistricts(state);
                if (districts == null) continue;

                foreach (var district in districts)
                {
                    if (ChannelExists(district.Name)) continue;

                    await Context.Guild.CreateTextChannelAsync(district.Name, x =>
                    {
                        x.Topic = $"Alerts for {district.Name}";
                        x.CategoryId = category.Id;
                        x.PermissionOverwrites = new[]
                        {
                            new Overwrite(Context.Guild.Id, PermissionTarget.Role,
                                new OverwritePermissions(sendMessages: PermValue.Deny))
                        };
                    });
                }
            }

            await ReplyAsync($"{Context.User.Mention}, I have created all channels!");
        }

        [Command("delete", RunMode = RunMode.Async)]
        [RequireOwner]
        public async Task DeleteAsync()
        {
            if (!await SentOnMasterGuild() || !await IsAdmin()) return;

            var states = await GetStates();
            if (states == null) return;

            await ReplyAsync("Deleting channels");

            foreach (var state in states)
            {
                var category = GetCategory(state.Name);
                if (category == null) return;

                foreach (var channel in category.Channels)
                {
                    await channel.DeleteAsync();
                }

                await category.DeleteAsync();
            }

            await ReplyAsync($"{Context.User.Mention}, I have deleted all channels!");
        }

        private async Task<bool> IsAdmin()
        {
            var permissions = Context.Guild.GetUser(Context.Client.CurrentUser.Id).GuildPermissions;
            if (permissions.Administrator)
            {
                return true;
            }

            await ReplyAsync("I do not have enough permissions to do this");
            return false;
        }

        private async Task<bool> SentOnMasterGuild()
        {
            var masterGuild = Context.Client.Guilds.FirstOrDefault(x => x.Id == 789070012307865632);
            if (Context.Guild.Id == masterGuild.Id)
            {
                return true;
            }

            await Context.User.SendMessageAsync($"Please run this command in {masterGuild.Name}");
            return false;
        }

        private bool ChannelExists(string name)
        {
            return Context.Guild.TextChannels.FirstOrDefault(x =>
                       string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase)) !=
                   null;
        }

        private SocketCategoryChannel GetCategory(string name)
        {
            return Context.Guild.CategoryChannels.FirstOrDefault(x => x.Name == name);
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

        private async Task<District[]> GetDistricts(State state)
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