
# CoWinDiscord
A discord bot made to get you vaccine alerts!
This does not have any releases, you need to compile it yourself (shit is hard coded, im sorry).

## How to configure
 - Add an appsettings.json file
 - The appsettings.json file has the following format:

```
{
    "defaultPrefix": "!",
    "token": "--your--token--here--"
}
```
- `defaultPrefix`: The prefix for the commands of the bot
- `token`: The token of your bot. You can get this from discord's developer portal
This is configured only for delhi, but you can change it by replacing the stateId in `AdminModule`, and in the `MainModule`. There is a 9, which is the state id of delhi. This can be changed to the stateId of your choice. (if you chose a state with a lot of districts, I am 100%  sure their api will temp block you).

## How to run
Host the bot, and invite it to *only 1 server*. Run `!do create` in a channel, and the bot should start making channels for each district in that server. It will also make 1 readonly channel: `#how-to-use`, and 2 community channels: `#feedback`, `#commands`. 

Now you can chill, as the bot sends message.

## Note:
I know this is a very shit idea (its hardcoded, you have to compile it, and its structure is rigid in general), but it was never made as a long term solution. No defending my self, this isnt clean or good code.