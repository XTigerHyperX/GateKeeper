using System;
using Discord;
using Discord.WebSocket;

namespace src.Services
{
    public class Mute
    {
        public SocketGuild Guild;
        public SocketGuildUser user;
        public IRole role;
        public DateTime End;
    }
}