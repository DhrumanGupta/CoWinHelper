using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace CoWinDiscord.Modules
{
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        [Command("help")]
        public async Task HelpAsync()
        {
            if (!Context.Channel.Name.Contains("command"))
            {
                await Context.Message.DeleteAsync();
                return;
            }

            var embed = new EmbedBuilder()
                .WithTitle("Bot Command List")
                .AddField("!about", "Get information about the bot")
                .AddField("!donate", "Get a link to donate and show support")
                .WithColor(Color.Blue)
                .Build();
            
            await ReplyAsync(null, false, embed);
        }
        
        [Command("donate")]
        public async Task DonateAsync()
        {
            if (!Context.Channel.Name.Contains("command"))
            {
                await Context.Message.DeleteAsync();
                return;
            }

            var embed = new EmbedBuilder()
                .WithTitle("Donate")
                .WithDescription(
                    "The bot is free to use, but you can show support for the bot by donating. There is no minimum limit to donate. [Link to Donate](https://berlm.me/#/donate)")
                .WithColor(Color.Blue)
                .WithTimestamp(DateTimeOffset.Now)
                .Build();
            
            await ReplyAsync(null, false, embed);
        }


        [Command("about")]
        public async Task AboutAsync()
        {
            if (!Context.Channel.Name.Contains("command"))
            {
                await Context.Message.DeleteAsync();
                return;
            }

            var embed = new EmbedBuilder()
                .WithDescription("This is a bot made to help those in need to get the vaccine. I made it because I felt it could help others in this challenging time.")
                .AddField("Framework Used:", "Discord.net", true)
                .WithColor(Color.Blue)
                .Build();
            
            await ReplyAsync(null, false, embed);
        }
    }
}