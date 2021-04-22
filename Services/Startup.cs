using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Gatekeeper.Modules;

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
            if (arg.Channel is SocketGuildChannel channel && arg.Author is IGuildUser author && channel.Guild.Id == _config.ServerId && channel.Id == _config.Verifier.ChannelId && !arg.Author.IsBot)
            {
                await arg.DeleteAsync();
                await author.AddRoleAsync(channel.Guild.Roles.FirstOrDefault(x => x.Id == _config.RoleId));
                await channel.Guild.GetTextChannel(_config.LogChannelId).SendMessageAsync(embed: new EmbedBuilder
                {
                    Title = "User Verified",
                    Color = Color.Green,
                    Fields = new List<EmbedFieldBuilder>
                    {
                        new EmbedFieldBuilder
                        {
                            Name = "User Verified",
                            Value = $"{author.Username}#{author.Discriminator} ({author.Id})",
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

                var (encrypted, decrypted) = Utils.GeneratePasteProofString();

                if (await arg.Channel.GetMessageAsync(_config.Verifier.MessageId) is IUserMessage message)
                {
                    await message.ModifyAsync(x =>
                    {
                        x.Embed = new EmbedBuilder
                        {
                            Description = $"To access the rest of the server, please type without copy pasting and with capitalization on:\n```fix\n{encrypted}\n```",
                            Color = new Color(250, 193, 27)
                        }.Build();
                    });
                }

                _config.Verifier.Code = decrypted;
                _config.Save();
            }

            if (arg.MentionedUsers.Any() && arg.Author is SocketGuildUser {IsBot: false} sender)
            {
                var users = arg.MentionedUsers;

                foreach (var u in users)
                {
                    if (!(u is SocketGuildUser user)) continue;
                    if (user.Roles.All(r => r.Id != 834473395135184948) || sender.Hierarchy >= user.Hierarchy) continue;
                    await sender.AddRoleAsync(sender.Guild.GetRole(833192988300804177));

                    if (!(arg.Channel is SocketGuildChannel c)) continue;
                    await arg.Channel.SendMessageAsync(embed: new EmbedBuilder // Channel user is currently in
                    {
                        Title = "User Muted",
                        Description = "Automod",
                        Color = Color.Red,
                        Fields = new List<EmbedFieldBuilder>
                        {
                            new EmbedFieldBuilder
                            {
                                Name = "User",
                                Value = $"{sender.Username}#{sender.Discriminator} ({sender.Id})",
                                IsInline = true
                            },
                            new EmbedFieldBuilder
                            {
                                Name = "Staff Member",
                                Value = "Gatekeeper",
                                IsInline = true
                            },
                            new EmbedFieldBuilder
                            {
                                Name = "Duration",
                                Value = "5 minutes",
                                IsInline = true
                            },
                            new EmbedFieldBuilder
                            {
                                Name = "Reason",
                                Value = "Pinging for no reason",
                                IsInline = false
                            }
                        }
                    }.Build());

                    await c.Guild.GetTextChannel(833395565214957578).SendMessageAsync(embed: new EmbedBuilder // Log
                    {
                        Title = "User Muted",
                        Description = "Automod",
                        Color = Color.Red,
                        Fields = new List<EmbedFieldBuilder>
                        {
                            new EmbedFieldBuilder
                            {
                                Name = "User",
                                Value = $"{sender.Username}#{sender.Discriminator} ({sender.Id})",
                                IsInline = true
                            },
                            new EmbedFieldBuilder
                            {
                                Name = "Staff Member",
                                Value = "Gatekeeper",
                                IsInline = true
                            },
                            new EmbedFieldBuilder
                            {
                                Name = "Duration",
                                Value = "5 minutes",
                                IsInline = true
                            },
                            new EmbedFieldBuilder
                            {
                                Name = "Reason",
                                Value = "Pinging for no reason",
                                IsInline = false
                            }
                        }
                    }.Build());
                }
            }
        }
    }
}