using Newtonsoft.Json;
using System.IO;

namespace src
{
    public static class ConfigExtensions
    {
        public static void Save(this JsonConfig conf)
        {
            File.WriteAllText(Program.ConfigFile, JsonConvert.SerializeObject(conf, Formatting.Indented));
        }
    }

    public partial class JsonConfig
    {
        public string BotToken;
        public string BotPrefix;
        public ulong ServerId;
        public ulong RoleId;
        public ulong LogChannelId;
        public Verifier Verifier;
    }

    public partial class Verifier
    {
        public ulong ChannelId;
        public ulong MessageId;
        public string Code;
    }
}
