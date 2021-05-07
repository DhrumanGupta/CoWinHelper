using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CoWinDiscord.Modules;
using Discord;
using Discord.Addons.Hosting;

namespace CoWinDiscord.Services
{
    public class CommandHandler : InitializedService
    {
        private readonly IServiceProvider _provider;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _service;
        private readonly IConfiguration _config;
        private readonly MainModule _mainModule;

        public CommandHandler(IServiceProvider provider, DiscordSocketClient client, CommandService service,
            IConfiguration config, MainModule mainModule)
        {
            _provider = provider;
            _client = client;
            _service = service;
            _config = config;
            _mainModule = mainModule;
        }

        public override async Task InitializeAsync(CancellationToken cancellationToken)
        {
            _client.MessageReceived += OnMessageReceived;
            _client.Ready += OnReady;
            await _service.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
        }

        private Task OnReady()
        {
            _mainModule.StartLoop();
            return Task.CompletedTask;
        }

        private async Task OnMessageReceived(SocketMessage arg)
        {
            if (arg is not SocketUserMessage {Source: MessageSource.User} message) return;

            var argPos = 0;
            var prefix = _config["defaultPrefix"];

            if (!message.HasStringPrefix(prefix, ref argPos) &&
                !message.HasMentionPrefix(_client.CurrentUser, ref argPos)) return;

            var context = new SocketCommandContext(_client, message);
            await _service.ExecuteAsync(context, argPos, _provider);
        }
    }
}