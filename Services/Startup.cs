using Discord;
using Discord.Commands;
using Discord.WebSocket;
using src.Modules;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace src.Services
{
    public class Startup
    {
        private readonly IServiceProvider _provider;
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private readonly JsonConfig _config;

        public Startup(IServiceProvider provider, DiscordSocketClient discord, CommandService commands, JsonConfig config)
        {
            _provider = provider;
            _discord = discord;
            _commands = commands;
            _config = config;

            _discord.Ready += OnDiscordReady;
            _discord.MessageReceived += OnDiscordMessageReceived;
        }

        public async Task StartAsync()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
            await _discord.LoginAsync(TokenType.Bot, _config.BotToken);
            await _discord.StartAsync();
        }

        private async Task OnDiscordReady()
        {
            var noRoleCount = 0;
            foreach (var guild in _discord.Guilds)
            {
                await guild.DownloadUsersAsync();
                foreach (var user in guild.Users)
                {
                    if (user.Roles.Count == 1)
                        noRoleCount++;
                }
            }

            if (noRoleCount == 0)
            {
                await _discord.SetGameAsync($"the door", null, ActivityType.Watching);
            }
            else 
                await _discord.SetGameAsync($"{noRoleCount} users' mind", null, (ActivityType)5);

            await _discord.SetStatusAsync(UserStatus.DoNotDisturb);
        }

        private async Task OnDiscordMessageReceived(SocketMessage arg)
        {
            if ((arg.Channel as SocketGuildChannel).Guild.Id == _config.ServerId && arg.Channel.Id == _config.Verifier.ChannelId && !arg.Author.IsBot)
            {
                await arg.DeleteAsync();
                if (Utils.VerifyPasteProofString(_config.Verifier.Code, arg.Content))
                {
                    await (arg.Author as IGuildUser).AddRoleAsync((arg.Channel as SocketGuildChannel).Guild.Roles.FirstOrDefault(x => x.Id == _config.RoleId));
                    if ((arg.Channel as SocketGuildChannel).Guild.GetTextChannel(_config.LogChannelId) is SocketTextChannel c)
                    {
                        await c.SendMessageAsync(string.Format("{0}#{1} ({2}) verified successfully with code `{3}`", arg.Author.Username, arg.Author.Discriminator, arg.Author.Id, _config.Verifier.Code));
                    }

                    (string encrypted, string decrypted) = Utils.GeneratePasteProofString();
                    var messageToEdit = await arg.Channel.GetMessageAsync(_config.Verifier.MessageId) as IUserMessage;
                    var lastEdited = messageToEdit.EditedTimestamp ?? messageToEdit.CreatedAt;

                    await messageToEdit.ModifyAsync(x =>
                    {
                        x.Embed = new EmbedBuilder()
                        {
                            Description = $"To access the rest of the server, please type without copy pasting with Caps on :\n```fix\n{encrypted}\n```",
                            Color = new Color(250, 193, 27)
                        }.Build();
                    });

                    // weird behavior here as "ModifyAsync" doesn't change "messageToEdit" if the message has never been edited before
                    // this cause the while exception to error out because "messageToEdit.EditedTimestamp" is null
                    //do
                    //{
                    //    await messageToEdit.ModifyAsync(x =>
                    //    {
                    //        x.Embed = new EmbedBuilder()
                    //        {
                    //            Description = $"To access the rest of the server, please type without copy pasting :\n```fix\n{encrypted}\n```",
                    //            Color = new Color(250, 193, 27)
                    //        }.Build();
                    //    });
                    //}
                    //while (lastEdited.Ticks == messageToEdit.EditedTimestamp.Value.Ticks); // check to see if it's actually modified

                    _config.Verifier.Code = decrypted;
                    _config.Save();
                }
            }
            /*
            else
            {
                if (!(arg.Author as SocketGuildUser).Roles.Any(r => r.Name == "Moderators") ||
                    arg.Content.ToLower() == ".rules")
                {
                    if ((arg.Content.StartsWith(".") || arg.Content.StartsWith("\\")) &&
                        arg.Content.ToLower() != ".rules")
                    {
                        if ((arg.Channel as SocketGuildChannel).Guild.GetTextChannel(833181135236366346) is
                            SocketTextChannel c)
                        {
                            await c.DeleteMessageAsync(arg);
                        }
                    }
                }
            }
            */
        }
    }
}