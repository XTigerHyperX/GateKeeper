using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.IO;
using System.Threading.Tasks;

namespace src.Services
{
    public class Logging
    {
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;

        public Logging(DiscordSocketClient discord, CommandService commands)
        {
            _discord = discord;
            _commands = commands;

            _discord.Log += OnLogAsync;
            _commands.Log += OnLogAsync;
        }

        private Task OnLogAsync(LogMessage msg)
        {
            if (!File.Exists(Program.LogFile))
            {
                File.Create(Program.LogFile).Dispose();
            }

            string message = string.Format("{0} [{1}] {2}: {3}\n", DateTime.UtcNow.ToString("MM/dd/yyyy HH:mm:ss"), msg.Severity.ToString(), msg.Source, msg.Exception?.ToString() ?? msg.Message);
            File.AppendAllText(Program.LogFile, message);
            return Console.Out.WriteAsync(message);
        }
    }
}
