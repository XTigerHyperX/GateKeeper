using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Gatekeeper.Modules;
using src.Modules;

namespace Gatekeeper.Services
{
    public class Startup
    {
        private readonly IServiceProvider _provider;
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private readonly JsonConfig _config;
        public static List<Mute> Mutes = new List<Mute>();

        private async Task MuteHandler()
        {
            var remove = new List<Mute>();
            foreach (var mute in Mutes)
            {
                if (DateTime.Now < mute.End)
                    continue;

                var guild = _discord.GetGuild(mute.Guild.Id);

                if (guild.GetRole(mute.Role.Id) == null)
                {
                    remove.Add(mute);
                    continue;
                }

                var role = guild.GetRole(mute.Role.Id);

                if (guild.GetUser(mute.User.Id) == null)
                {
                    remove.Add(mute);
                    continue;
                }

                var user = guild.GetUser(mute.User.Id);

                if (role.Position > guild.CurrentUser.Hierarchy)
                {
                    remove.Add(mute);
                    continue;
                }

                await user.RemoveRoleAsync(mute.Role);
                remove.Add(mute);
                var gatelog = _discord.GetGuild(681940030897651773).GetTextChannel(833395565214957578);

                await gatelog.SendMessageAsync(embed: new EmbedBuilder
                {
                    Title = "User Unmuted",
                    Color = Color.Green,
                    Fields = new List<EmbedFieldBuilder>
                    {
                        new EmbedFieldBuilder
                        {
                            Name = "Muted User",
                            Value = $"{mute.User.Username}#{mute.User.Discriminator} ({mute.User.Id})",
                            IsInline = true
                        },
                        new EmbedFieldBuilder
                        {
                            Name = "Staff Member",
                            Value = "Gatekeeper",
                            IsInline = true
                        }
                    }
                }.Build());
            }

            Mutes = Mutes.Except(remove).ToList();
            await Task.Delay(TimeSpan.FromSeconds(10));
            await MuteHandler();
        }

        public Startup(IServiceProvider provider, DiscordSocketClient discord, CommandService commands, JsonConfig config)
        {
            _provider = provider;
            _discord = discord;
            _commands = commands;
            _config = config;

            _discord.Ready += OnDiscordReady;
            _discord.MessageReceived += OnDiscordMessageReceived;
            var newtask = new Task(async () => await MuteHandler());
            newtask.Start();
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
                noRoleCount += guild.Users.Count(user => user.Roles.Count == 1);
            }

            if (noRoleCount == 0)
            {
                await _discord.SetGameAsync($"the door", null, ActivityType.Watching);
            }
            else
            {
                await _discord.SetGameAsync($"{noRoleCount} users' mind", null, (ActivityType) 5);
            }

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
                        await c.SendMessageAsync(embed: new EmbedBuilder
                        {
                            Title = "User Verified",
                            Color = Color.Green,
                            Fields = new List<EmbedFieldBuilder>
                            {new EmbedFieldBuilder
                                {
                                    Name = "User Verified",
                                    Value = $"{arg.Author.Username}#{arg.Author.Discriminator} ({arg.Author.Id})",
                                    IsInline = true
                                },
                                new EmbedFieldBuilder
                                {
                                    Name = "Code Used",
                                    Value = _config.Verifier.Code,
                                    IsInline = true
                                }
                            }
                        }.Build());
                    }
                    (string encrypted, string decrypted) = Utils.GeneratePasteProofString();
                    var messageToEdit = await arg.Channel.GetMessageAsync(_config.Verifier.MessageId) as IUserMessage;

                    await messageToEdit!.ModifyAsync(x =>
                    {
                        x.Embed = new EmbedBuilder()
                        {
                            Description = $"To access the rest of the server, please type without copy pasting with Caps on :\n```fix\n{encrypted}\n```",
                            Color = new Color(250, 193, 27)
                        }.Build();
                    });
                    _config.Verifier.Code = decrypted;
                    _config.Save();
                }
            }
        }
    }
}