using System;
using System.IO;
using System.Linq;
using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using Discord.WebSocket;
using src.Services;

namespace src.Modules
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
            (string encrypted, string decrypted) = Utils.GeneratePasteProofString();
            var message = await this.ReplyAsync("", false, new EmbedBuilder()
            {
                Description = $"To access the rest of the server, please type **without copy pasting and with CAPS on** :\n```fix\n{encrypted}\n```",
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
            await this.Context.Message.DeleteAsync();
            await this.ReplyAsync($"`{_config.Verifier.Code}`");
        }
#region ban
        [RequireUserPermission(GuildPermission.BanMembers, ErrorMessage = "No")]
        [Command("ban")]
        public async Task ban(IGuildUser user = null , [Remainder] string reason = null)
        {
            if (user == null)
            {
                await ReplyAsync("Specify a user!");
                return;
            }
            if (reason == null) reason = "Non Specified";
            await Context.Guild.AddBanAsync(user, 1, reason);
            await ReplyAsync($"{user.Username}{user.Discriminator} was banned \n **Reason : ** {reason}");
            
            ITextChannel gatelog = Context.Client.GetChannel(833395565214957578) as ITextChannel;
            await gatelog.SendMessageAsync($"{user.Username}{user.Discriminator} ({user.Id}) was **banned** **by** {Context.User.Username}{Context.User.Discriminator} **Reason : ** {reason}");
        }
#endregion

#region unban
        [RequireUserPermission(GuildPermission.BanMembers, ErrorMessage = "No")]
        [Command("unban")]
        public async Task unban(ulong userID)
        {
            await Context.Guild.RemoveBanAsync(userID);
            await ReplyAsync($"Unbanned {userID} ! <:Gatekeeper:833756060032303174>");

            ITextChannel gatelog = Context.Client.GetChannel(833395565214957578) as ITextChannel;
            await gatelog.SendMessageAsync(
                    $"{userID} was **unbanned** **by** {Context.User.Username}{Context.User.Discriminator}");
        }
#endregion

#region rules
[RequireUserPermission(GuildPermission.SendMessages, ErrorMessage = "No")]
        [Command("rules")]
        public async Task rules()
        {
            string rules = File.ReadAllText(@"C:\Users\XTigerHyperX\Desktop\Gatekeeper-main\Rules.txt");
            var EmbedBuilder = new EmbedBuilder()
                .WithTitle("Welcome to The Tiger's Den")
                .WithDescription(rules)
                .WithColor(36, 63, 115)
                .WithFooter(footer =>
                {
                    footer
                        .WithText("Gatekeeper")
                        .WithIconUrl("https://i.imgur.com/rVB8XsP.png");
                });
            Embed embed = EmbedBuilder.Build();
            await ReplyAsync(embed: embed);
        }
#endregion

#region kick
        [RequireUserPermission(GuildPermission.KickMembers, ErrorMessage = "i'd rather play the reverse card <:Gatekeeper:833756060032303174> ")]
        [Command("kick")]
        public async Task kick(IGuildUser user = null, [Remainder] string reason = null)
        {
            if (user == null)
            {
                await ReplyAsync("Specify a user!");
                return;
            }

            if (reason == null) reason = "Non Specified";

            await user.KickAsync(reason);
            await ReplyAsync($"{user.Username}{user.Discriminator} was Kicked \n **Reason : ** {reason}");

            ITextChannel gatelog = Context.Client.GetChannel(833395565214957578) as ITextChannel;
            await gatelog.SendMessageAsync($"{user.Username}{user.Discriminator} ({user.Id}) was **kicked** **by** {Context.User.Username}{Context.User.Discriminator} **Reason : ** {reason}");
        }
        #endregion

        
        [RequireUserPermission(GuildPermission.KickMembers , ErrorMessage = "I Can't do that")]
        [Command("mute")]
        public async Task mute(SocketGuildUser user = null , int? minutes = null ,[Remainder] string reason = null)
        {
            if (Context.Message.MentionedUsers.Any())
            {
                user = (Context.Message.MentionedUsers.First() as SocketGuildUser);
            }
            if (user == null)
            {
                await ReplyAsync("Specify who to mute <:Gatekeeper:833756060032303174> ");
            }
            if (user.Hierarchy > Context.Guild.CurrentUser.Hierarchy)
            {
                await Context.Channel.SendMessageAsync("I Can't do that");
                return;
            }
            var role = (Context.Guild as IGuild).Roles.FirstOrDefault(x => x.Id == 833192988300804177);
            if (role.Position > Context.Guild.CurrentUser.Hierarchy)
            {
                await Context.Channel.SendMessageAsync("The user have a higher position than me");
                return;
            }
            if (user.Roles.Contains(role))
            {
                await Context.Channel.SendMessageAsync("This user is already muted");
                return;
            }

            if (reason == null) reason = "Non Specified";
            if (minutes != null)
            {
                //await role.ModifyAsync(x => x.Position = Context.Guild.CurrentUser.Hierarchy);
                Startup.Mutes.Add(new Mute
                {
                    Guild = Context.Guild, user = user, End = DateTime.Now + TimeSpan.FromMinutes((int) minutes),
                    role = role
                });
                await user.AddRoleAsync(user.Guild.Roles.FirstOrDefault(x => x.Id == 833192988300804177));
                await ReplyAsync(
                    $"{user.Username}{user.Discriminator} **was muted** **Duration : **{minutes} minutes **Reason** {reason}");
                ITextChannel gatelog = Context.Client.GetChannel(833395565214957578) as ITextChannel;
                await gatelog.SendMessageAsync(
                    $"{user.Username}#{user.Discriminator} ({user.Id}) was **muted** for {minutes} minutes **by** {Context.User.Username}{Context.User.Discriminator} **Reason : {reason}");
            }
            else if (minutes == null)
            {
                await user.AddRoleAsync(user.Guild.Roles.FirstOrDefault(x => x.Id == 833192988300804177));
                await ReplyAsync(
                    $"{user.Username}#{user.Discriminator} **was muted** **for non specified time :** **Reason :** {reason}");
                ITextChannel gatelog = Context.Client.GetChannel(833395565214957578) as ITextChannel;
                await gatelog.SendMessageAsync(
                    $"{user.Username}#{user.Discriminator} ({user.Id}) **was muted** for non specified time : **by** {Context.User.Username}#{Context.User.Discriminator} **Reason :** {reason}");
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
        public async Task unmute(SocketGuildUser user)
        {
            var role = (Context.Guild as IGuild).Roles.FirstOrDefault(x => x.Id == 833192988300804177);
            if (role == null)
            {
                await Context.Channel.SendMessageAsync("The user is not muted");
                return;
            }
            if (role.Position > Context.Guild.CurrentUser.Hierarchy)
            {
                await Context.Channel.SendMessageAsync("The user have a higher position than me");
                return;
            }
            if (!user.Roles.Contains(role))
            {
                await Context.Channel.SendMessageAsync("This user is not muted");
                return;
            }
            //await role.ModifyAsync(x => x.Position = Context.Guild.CurrentUser.Hierarchy);
            await user.RemoveRoleAsync(role);
            await ReplyAsync($"{user.Username}{user.Discriminator} **was unmuted**");
            ITextChannel gatelog = Context.Client.GetChannel(833395565214957578) as ITextChannel;
            await gatelog.SendMessageAsync(
                $"{user.Username}{user.Discriminator} ({user.Id}) was **unmuted** **by** {Context.User.Username}{Context.User.Discriminator}");
        }

        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("alive?")]
        public async Task unmute()
        {
            await ReplyAsync("Yes i'm alive ! <:Gatekeeper:833756060032303174> ");
        }

        [RequireUserPermission(GuildPermission.SendMessages)]
        [Command("help")]
        public async Task help()
        {
            var Embed = new EmbedBuilder()
                .WithTitle("Help")
                .WithColor(23, 56, 94)
                .WithDescription(
                    "The Prefix for Gatekeeper is `.` \n " +
                    "**ban** \n bye <:PepeHappy:833198633025273856> \n **unban** \n unbans user \n **kick** \n kicks user" +
                    "\n **mute** \n provide user and add minutes and reason if needed \n **unmute** \n unmutes user " +
                    "\n **alive** \n to check bot presence \n **rules** \n my favorite part <:Gatekeeper:833756060032303174> \n **help** \n provides help commands")
                .WithFooter(footer =>
                {
                    footer
                        .WithText("Gatekeeper")
                        .WithIconUrl("https://imgur.com/rVB8XsP.png");
                });
            Embed embd = Embed.Build();
            ReplyAsync(embed: embd);
        }
        
    }
}
