using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Gatekeeper.Services;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Gatekeeper
{
    public static class Program
    {
        private static JsonConfig Configuration { get; set; }
        private static readonly string OutputDirectory = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule?.FileName);

        private static readonly string ConfigDirectory = Path.Combine(OutputDirectory, "Config");
        public static readonly string ConfigFile = Path.Combine(ConfigDirectory, "configuration.json");
        private static readonly string LogDirectory = Path.Combine(OutputDirectory, "Logs");
        public static readonly string LogFile = Path.Combine(LogDirectory, $"Gatekeeper_{DateTime.UtcNow:yyyy-MM-dd}.txt");

        public static Task Main(string[] args) => RunAsync(args);
        private static async Task RunAsync(string[] _)
        {
            Setup();
            var services = new ServiceCollection()
                .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig {LogLevel = LogSeverity.Verbose, MessageCacheSize = 1000}))
                .AddSingleton(new CommandService(new CommandServiceConfig {LogLevel = LogSeverity.Verbose, DefaultRunMode = RunMode.Async}))
                .AddSingleton<Command>()
                .AddSingleton<Logging>()
                .AddSingleton<Startup>()
                .AddSingleton<Random>()
                .AddSingleton(Configuration);

            var provider = services.BuildServiceProvider();
            provider.GetRequiredService<Command>();
            provider.GetRequiredService<Logging>();

            await provider.GetRequiredService<Startup>().StartAsync();
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
                var token = Console.ReadLine().Trim();
                Console.Out.WriteLine("2. Bot Prefix:");
                var prefix = Console.ReadLine().Trim();
                Console.Out.WriteLine("3. Server Id:");
                var server = Console.ReadLine().Trim();
                Console.Out.WriteLine("4. Role Id:");
                var role = Console.ReadLine().Trim();
                Console.Out.WriteLine("5. Verification Channel Id:");
                var channel = Console.ReadLine().Trim();
                Console.Out.WriteLine("6. Log Channel Id:");
                var log = Console.ReadLine().Trim();
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