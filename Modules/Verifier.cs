﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using Discord.WebSocket;
using Gatekeeper.Services;

namespace Gatekeeper.Modules
{
    public class Verifier : ModuleBase<SocketCommandContext>
    {
        private readonly JsonConfig _config;

        public Verifier(JsonConfig config)
        {
            _config = config;
        }

        [RequireOwner]
        [Command("generate")]
        public async Task Generate()
        {
            var (encrypted, decrypted) = Utils.GeneratePasteProofString();
            var message = await ReplyAsync(embed: new EmbedBuilder
            {
                Description = $"To access the rest of the server, please type without copy pasting and with capitalization on:\n```fix\n{encrypted}\n```",
                Color = new Color(250, 193, 27)
            }.Build());

            _config.Verifier.Code = decrypted;
            _config.Verifier.MessageId = message.Id;
            _config.Save();
        }

        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("code")]
        public async Task Code()
        {
            await Context.Message.DeleteAsync();
            await ReplyAsync($"`{_config.Verifier.Code}`");
        }

        #region ban

        [RequireUserPermission(GuildPermission.BanMembers, ErrorMessage = "No")]
        [Command("ban")]
        public async Task Ban(IGuildUser user = null, [Remainder] string reason = null)
        {
            if (user == null)
            {
                await ReplyAsync(embed: new EmbedBuilder
                {
                    Title = "Error",
                    Color = Color.Red,
                    Description = "ERROR: Please specify a user to ban!",
                    Footer = new EmbedFooterBuilder
                    {
                        Text = "Gatekeeper",
                        IconUrl = "https://i.imgur.com/rVB8XsP.png"
                    }
                }.Build());
                return;
            }

            reason ??= "Unspecified";
            await Context.Guild.AddBanAsync(user, 1, reason);
            await ReplyAsync(embed: new EmbedBuilder
            {
                Title = "User Banned",
                Color = Color.Red,
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder
                    {
                        Name = "User",
                        Value = $"{user.Username}#{user.Discriminator} ({user.Id})",
                        IsInline = true
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Staff Member",
                        Value = $"{Context.User.Username}#{Context.User.Discriminator} ({Context.User.Id})",
                        IsInline = true
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Reason",
                        Value = reason,
                        IsInline = false
                    }
                }
            }.Build());

            await Context.Guild.GetTextChannel(833395565214957578).SendMessageAsync(embed: new EmbedBuilder
            {
                Title = "User Banned",
                Color = Color.Red,
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder
                    {
                        Name = "User",
                        Value = $"{user.Username}#{user.Discriminator} ({user.Id})",
                        IsInline = true
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Staff Member",
                        Value = $"{Context.User.Username}#{Context.User.Discriminator} ({Context.User.Id})",
                        IsInline = true
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Reason",
                        Value = reason,
                        IsInline = false
                    }
                }
            }.Build());
        }

        #endregion

        #region unban

        [RequireUserPermission(GuildPermission.BanMembers, ErrorMessage = "No")]
        [Command("unban")]
        public async Task Unban(ulong userId)
        {
            await Context.Guild.RemoveBanAsync(userId);
            var user = Context.Client.GetUser(userId);

            await ReplyAsync(embed: new EmbedBuilder
            {
                Title = "User Unbanned",
                Color = Color.Green,
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder
                    {
                        Name = "User",
                        Value = $"{user.Username}#{user.Discriminator} ({user.Id})",
                        IsInline = true
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Staff Member",
                        Value = $"{Context.User.Username}#{Context.User.Discriminator} ({Context.User.Id})",
                        IsInline = true
                    }
                }
            }.Build());

            await Context.Guild.GetTextChannel(833395565214957578).SendMessageAsync(embed: new EmbedBuilder
            {
                Title = "User Unbanned",
                Color = Color.Green,
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder
                    {
                        Name = "User",
                        Value = $"{user.Username}#{user.Discriminator} ({user.Id})",
                        IsInline = true
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Staff Member",
                        Value = $"{Context.User.Username}#{Context.User.Discriminator} ({Context.User.Id})",
                        IsInline = true
                    }
                }
            }.Build());
        }

        #endregion

        #region rules

        [RequireUserPermission(GuildPermission.SendMessages, ErrorMessage = "No")]
        [Command("rules")]
        public async Task Rules()
        {
            await ReplyAsync(embed: new EmbedBuilder
            {
                Title = "Welcome to The Tiger's Den",
                Description = await File.ReadAllTextAsync(@"C:\Users\XTigerHyperX\Desktop\Gatekeeper-main\Rules.txt"),
                Color = new Color(36, 63, 115),
                Footer = new EmbedFooterBuilder
                {
                    Text = "Gatekeeper",
                    IconUrl = "https://i.imgur.com/rVB8XsP.png"
                }
            }.Build());
        }

        #endregion

        #region kick

        [RequireUserPermission(GuildPermission.KickMembers, ErrorMessage = "i'd rather play the reverse card <:Gatekeeper:833756060032303174> ")]
        [Command("kick")]
        public async Task Kick(IGuildUser user = null, [Remainder] string reason = null)
        {
            if (user == null)
            {
                await ReplyAsync(embed: new EmbedBuilder
                {
                    Title = "Error",
                    Color = Color.Red,
                    Description = "ERROR: Please specify a user to kick!",
                    Footer = new EmbedFooterBuilder
                    {
                        Text = "Gatekeeper",
                        IconUrl = "https://i.imgur.com/rVB8XsP.png"
                    }
                }.Build());
                return;
            }

            reason ??= "Unspecified";
            await user.KickAsync(reason);
            await ReplyAsync(embed: new EmbedBuilder
            {
                Title = "User Kicked",
                Color = Color.Orange,
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder
                    {
                        Name = "User",
                        Value = $"{user.Username}#{user.Discriminator} ({user.Id})",
                        IsInline = true
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Staff Member",
                        Value = $"{Context.User.Username}#{Context.User.Discriminator} ({Context.User.Id})",
                        IsInline = true
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Reason",
                        Value = reason,
                        IsInline = false
                    }
                }
            }.Build());

            await Context.Guild.GetTextChannel(833395565214957578).SendMessageAsync(embed: new EmbedBuilder
            {
                Title = "User Kicked",
                Color = Color.Orange,
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder
                    {
                        Name = "User",
                        Value = $"{user.Username}#{user.Discriminator} ({user.Id})",
                        IsInline = true
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Staff Member",
                        Value = $"{Context.User.Username}#{Context.User.Discriminator} ({Context.User.Id})",
                        IsInline = true
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Reason",
                        Value = reason,
                        IsInline = false
                    }
                }
            }.Build());
        }

        #endregion

        [RequireUserPermission(GuildPermission.KickMembers, ErrorMessage = "I can't do that")]
        [Command("mute")]
        public async Task Mute(SocketGuildUser user = null, int? minutes = null, [Remainder] string reason = null)
        {
            if (Context.Message.MentionedUsers.Any() && Context.Message.MentionedUsers.First() is SocketGuildUser u)
                user = u;

            if (user == null)
            {
                await ReplyAsync(embed: new EmbedBuilder
                {
                    Title = "Error",
                    Color = Color.Red,
                    Description = "ERROR: Please specify a user to mute!",
                    Footer = new EmbedFooterBuilder
                    {
                        Text = "Gatekeeper",
                        IconUrl = "https://i.imgur.com/rVB8XsP.png"
                    }
                }.Build());
                return;
            }

            if (user.Hierarchy > Context.Guild.CurrentUser.Hierarchy)
            {
                await ReplyAsync(embed: new EmbedBuilder
                {
                    Title = "Error",
                    Color = Color.Red,
                    Description = "ERROR: User is higher than my permission level.",
                    Footer = new EmbedFooterBuilder
                    {
                        Text = "Gatekeeper",
                        IconUrl = "https://i.imgur.com/rVB8XsP.png"
                    }
                }.Build());
                return;
            }

            var role = Context.Guild.Roles.FirstOrDefault(x => x.Id == 833192988300804177);
            if (role != null)
            {
                if (Context.Guild.Roles.Count > 0 && role.Position > Context.Guild.CurrentUser.Hierarchy)
                {
                    await ReplyAsync(embed: new EmbedBuilder
                    {
                        Title = "Error",
                        Color = Color.Red,
                        Description = "ERROR: User is higher than my permission level.",
                        Footer = new EmbedFooterBuilder
                        {
                            Text = "Gatekeeper",
                            IconUrl = "https://i.imgur.com/rVB8XsP.png"
                        }
                    }.Build());
                    return;
                }

                if (user.Roles.Contains(role))
                {
                    await ReplyAsync(embed: new EmbedBuilder
                    {
                        Title = "Error",
                        Color = Color.Red,
                        Description = "ERROR: User is already muted.",
                        Footer = new EmbedFooterBuilder
                        {
                            Text = "Gatekeeper",
                            IconUrl = "https://i.imgur.com/rVB8XsP.png"
                        }
                    }.Build());
                    return;
                }

                reason ??= "Unspecified";
                minutes ??= 0;
                Startup.Mutes.Add(new Mute
                {
                    Guild = Context.Guild,
                    User = user,
                    End = DateTime.Now + TimeSpan.FromMinutes((int) minutes),
                    Role = role
                });

                await user.AddRoleAsync(role);

                await ReplyAsync(embed: new EmbedBuilder
                {
                    Title = "User Muted",
                    Color = Color.Red,
                    Fields = new List<EmbedFieldBuilder>
                    {
                        new EmbedFieldBuilder
                        {
                            Name = "User",
                            Value = $"{user.Username}#{user.Discriminator} ({user.Id})",
                            IsInline = true
                        },
                        new EmbedFieldBuilder
                        {
                            Name = "Staff Member",
                            Value = $"{Context.User.Username}#{Context.User.Discriminator} ({Context.User.Id})",
                            IsInline = true
                        },
                        new EmbedFieldBuilder
                        {
                            Name = "Duration",
                            Value = minutes == 0 ? minutes.ToString() : "Unspecified",
                            IsInline = true
                        },
                        new EmbedFieldBuilder
                        {
                            Name = "Reason",
                            Value = reason,
                            IsInline = false
                        }
                    }
                }.Build());

                await Context.Guild.GetTextChannel(833395565214957578).SendMessageAsync(embed: new EmbedBuilder // Log
                {
                    Title = "User Muted",
                    Color = Color.Red,
                    Fields = new List<EmbedFieldBuilder>
                    {
                        new EmbedFieldBuilder
                        {
                            Name = "User",
                            Value = $"{user.Username}#{user.Discriminator} ({user.Id})",
                            IsInline = true
                        },
                        new EmbedFieldBuilder
                        {
                            Name = "Staff Member",
                            Value = $"{Context.User.Username}#{Context.User.Discriminator} ({Context.User.Id})",
                            IsInline = true
                        },
                        new EmbedFieldBuilder
                        {
                            Name = "Duration",
                            Value = minutes == 0 ? minutes.ToString() : "Unspecified",
                            IsInline = true
                        },
                        new EmbedFieldBuilder
                        {
                            Name = "Reason",
                            Value = reason,
                            IsInline = false
                        }
                    }
                }.Build());
            }
        }

        /*

        [RequireUserPermission(GuildPermission.MuteMembers,
            ErrorMessage = "No")]

        [Command("unmute")]
        public async Task unmute(IGuildUser user = null, [Remainder] string reason = null)
        {
            if (user == null)
            {
                await ReplyAsync("who do i unmute ? <:Gatekeeper:833756060032303174>");
                return;
            }

                await user.RemoveRoleAsync(user.Guild.Roles.FirstOrDefault(x => x.Id == 833192988300804177));
                await ReplyAsync($"{user.Username}{user.Discriminator} was unmuted");
                ITextChannel gatelog = Context.Client.GetChannel(833395565214957578) as ITextChannel;
                await gatelog.SendMessageAsync(
                    $"{user.Username}{user.Discriminator} ({user.Id}) was **unmuted** **by** {Context.User.Username}{Context.User.Discriminator}");
        }
        */

        [RequireUserPermission(GuildPermission.KickMembers)]
        [Command("unmute")]
        public async Task Unmute(SocketGuildUser user = null)
        {
            if (user == null)
            {
                await ReplyAsync(embed: new EmbedBuilder
                {
                    Title = "Error",
                    Color = Color.Red,
                    Description = "ERROR: Please specify a user to unmute!",
                    Footer = new EmbedFooterBuilder
                    {
                        Text = "Gatekeeper",
                        IconUrl = "https://i.imgur.com/rVB8XsP.png"
                    }
                }.Build());
                return;
            }
            
            var role = Context.Guild.Roles.FirstOrDefault(x => x.Id == 833192988300804177);
            if (role != null)
            {
                if (role.Position > Context.Guild.CurrentUser.Hierarchy)
                {
                    await ReplyAsync(embed: new EmbedBuilder
                    {
                        Title = "Error",
                        Color = Color.Red,
                        Description = "ERROR: Role is higher than my permission level.",
                        Footer = new EmbedFooterBuilder
                        {
                            Text = "Gatekeeper",
                            IconUrl = "https://i.imgur.com/rVB8XsP.png"
                        }
                    }.Build());
                    return;
                }

                if (!user.Roles.Contains(role))
                {
                    await ReplyAsync(embed: new EmbedBuilder
                    {
                        Title = "Error",
                        Color = Color.Red,
                        Description = "ERROR: User is not muted.",
                        Footer = new EmbedFooterBuilder
                        {
                            Text = "Gatekeeper",
                            IconUrl = "https://i.imgur.com/rVB8XsP.png"
                        }
                    }.Build());
                    return;
                }

                await user.RemoveRoleAsync(role);
                await ReplyAsync(embed: new EmbedBuilder
                {
                    Title = "User Unmuted",
                    Color = Color.Green,
                    Fields = new List<EmbedFieldBuilder>
                    {
                        new EmbedFieldBuilder
                        {
                            Name = "Muted User",
                            Value = $"{user.Username}#{user.Discriminator} ({user.Id})",
                            IsInline = true
                        },
                        new EmbedFieldBuilder
                        {
                            Name = "Staff Member",
                            Value = $"{Context.User.Username}#{Context.User.Discriminator} ({Context.User.Id})",
                            IsInline = true
                        }
                    }
                }.Build());
                
                await Context.Guild.GetTextChannel(833395565214957578).SendMessageAsync(embed: new EmbedBuilder // Log
                {
                    Title = "User Unmuted",
                    Color = Color.Green,
                    Fields = new List<EmbedFieldBuilder>
                    {
                        new EmbedFieldBuilder
                        {
                            Name = "Muted User",
                            Value = $"{user.Username}#{user.Discriminator} ({user.Id})",
                            IsInline = true
                        },
                        new EmbedFieldBuilder
                        {
                            Name = "Staff Member",
                            Value = $"{Context.User.Username}#{Context.User.Discriminator} ({Context.User.Id})",
                            IsInline = true
                        }
                    }
                }.Build());
            }
        }

        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("alive?")]
        public async Task Alive()
        {
            await ReplyAsync("I'm alive and reporting for duty! <:Gatekeeper:833756060032303174>");
        }

        [RequireUserPermission(GuildPermission.SendMessages)]
        [Command("help")]
        public async Task Help()
        {
            await ReplyAsync(embed: new EmbedBuilder
            {
                Title = "Help",
                Color = new Color(23, 56, 94),
                Description = "The Prefix for Gatekeeper is `.` \n " +
                              "**ban** \n bye <:PepeHappy:833198633025273856> \n **unban** \n unbans user \n **kick** \n kicks user" +
                              "\n **mute** \n provide user and add minutes and reason if needed \n **unmute** \n unmutes user " +
                              "\n **alive** \n to check bot presence \n **rules** \n my favorite part <:Gatekeeper:833756060032303174> \n **help** \n provides help commands",
                Footer = new EmbedFooterBuilder
                {
                    Text = "Gatekeeper",
                    IconUrl = "https://imgur.com/rVB8XsP.png"
                }
            }.Build());
        }
    }
}