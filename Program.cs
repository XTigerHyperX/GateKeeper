using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace src
{
    public static class Program
    {
        public static JsonConfig Configuration { get; set; }
        public static readonly string OutputDirectory = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

        public static readonly string ConfigDirectory = Path.Combine(OutputDirectory, "Config");
        public static readonly string ConfigFile = Path.Combine(ConfigDirectory, "configuration.json");
        public static readonly string LogDirectory = Path.Combine(OutputDirectory, "Logs");
        public static readonly string LogFile = Path.Combine(LogDirectory, $"Gatekeeper_{DateTime.UtcNow:yyyy-MM-dd}.txt");

        public static Task Main(string[] args) => RunAsync(args);
        public static async Task RunAsync(string[] _)
        {
            Setup();
            IServiceCollection services = new ServiceCollection()
                    .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig { LogLevel = LogSeverity.Verbose, MessageCacheSize = 1000 }))
                    .AddSingleton(new CommandService(new CommandServiceConfig { LogLevel = LogSeverity.Verbose, DefaultRunMode = RunMode.Async }))
                    .AddSingleton<Services.Command>()
                    .AddSingleton<Services.Logging>()
                    .AddSingleton<Services.Startup>()
                    .AddSingleton<Random>()
                    .AddSingleton(Configuration);

            ServiceProvider provider = services.BuildServiceProvider();
            provider.GetRequiredService<Services.Command>();
            provider.GetRequiredService<Services.Logging>();

            await provider.GetRequiredService<Services.Startup>().StartAsync();
            await Task.Delay(-1);
        }

        private static void Setup()
        {
            Directory.CreateDirectory(ConfigDirectory);
            Directory.CreateDirectory(LogDirectory);

            if (!File.Exists(ConfigFile))
            {
                File.Create(ConfigFile).Dispose();
                Console.Out.WriteLine("Configuration not found, setup incoming...");
                Console.Out.WriteLine("1. Bot Token:");
                string token = Console.ReadLine().Trim();
                Console.Out.WriteLine("2. Bot Prefix:");
                string prefix = Console.ReadLine().Trim();
                Console.Out.WriteLine("3. Server Id:");
                string server = Console.ReadLine().Trim();
                Console.Out.WriteLine("4. Role Id:");
                string role = Console.ReadLine().Trim();
                Console.Out.WriteLine("5. Verification Channel Id:");
                string channel = Console.ReadLine().Trim();
                Console.Out.WriteLine("6. Log Channel Id:");
                string log = Console.ReadLine().Trim();
                Console.Clear();

                Configuration = new JsonConfig
                {
                    BotToken = token,
                    BotPrefix = prefix,
                    ServerId = ulong.Parse(server),
                    RoleId = ulong.Parse(role),
                    LogChannelId = ulong.Parse(log),
                    Verifier = new Verifier
                    {
                        ChannelId = ulong.Parse(channel)
                    }
                };
                Configuration.Save();
            }
            else
            {
                Configuration = JsonConvert.DeserializeObject<JsonConfig>(File.ReadAllText(ConfigFile));
            }
        }
    }
}
