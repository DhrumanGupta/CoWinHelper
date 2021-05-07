using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace CoWinDiscord.Modules
{
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        [Command("donate")]
        public async Task DonateAsync()
        {
            if (Context.Channel.Id != 840479850123886592)
            {
                await Context.Message.DeleteAsync();
                return;
            }

            var embed = new EmbedBuilder()
                .WithTitle("Donate")
                .WithDescription(
                    "The bot is free to use, but you can show support for the bot by donating. There is no minimum limit to donate. [Link to Donate](https://selfregistration.cowin.gov.in/)")
                .WithColor(Color.Blue)
                .WithTimestamp(DateTimeOffset.Now)
                .Build();
            
            await ReplyAsync(null, false, embed);
        }
    }
}