using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace src.Services
{
    public class Command
    {
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private readonly IServiceProvider _provider;
        private readonly JsonConfig _config;

        public Command(DiscordSocketClient discord, CommandService commands, IServiceProvider provider, JsonConfig config)
        {
            _discord = discord;
            _commands = commands;
            _provider = provider;
            _config = config;

            _discord.MessageReceived += OnMessageReceivedAsync;
        }

        private async Task OnMessageReceivedAsync(SocketMessage s)
        {
            if (!(s is SocketUserMessage msg)) return;
            if (msg.Author.Id == _discord.CurrentUser.Id) return;

            var context = new SocketCommandContext(_discord, msg);

            int argPos = 0;
            if (msg.HasStringPrefix(_config.BotPrefix, ref argPos) || msg.HasMentionPrefix(_discord.CurrentUser, ref argPos))
            {
                var result = await _commands.ExecuteAsync(context, argPos, _provider);
                if (!result.IsSuccess) Console.WriteLine(result.ErrorReason);
                if (result.Error.Equals(CommandError.UnmetPrecondition))
                    await s.Channel.SendMessageAsync(result.ErrorReason);
            }
        }
    }
}
