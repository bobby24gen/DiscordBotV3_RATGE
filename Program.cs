using Discord.Interactions;
using Discord.WebSocket;
using Lavalink4NET.Extensions;
using DiscordBotV3;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Discord;
using System.Globalization;
using System.Reflection;
using Lavalink4NET;
using Discord.Commands;
using Discord.Commands.Builders;

namespace DiscordBotV3;
public class Program
{
    private static IConfiguration? _configuration;
    private static IServiceProvider? _services;

    private static readonly DiscordSocketConfig _socketConfig = new()
    {
        GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers | GatewayIntents.MessageContent | GatewayIntents.GuildVoiceStates,
        AlwaysDownloadUsers = true,
        MessageCacheSize = 100,
    };

    private static readonly InteractionServiceConfig _interactionServiceConfig = new()
    {
        LocalizationManager = new ResxLocalizationManager("InteractionFramework.Resources.CommandLocales", Assembly.GetEntryAssembly(),
            new CultureInfo("en-US"), new CultureInfo("ru"))
    };

    private static readonly CommandServiceConfig _commandConfig = new()
    {
        LogLevel = LogSeverity.Info,
        CaseSensitiveCommands = false,        
    };

    public static async Task Main(string[] args)
    {
        _configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables(prefix: "DC_")
            .AddJsonFile("appsettings.json", optional: true)            
            .Build();

        _services = new ServiceCollection()
            .AddSingleton(_configuration)
            .AddSingleton(_socketConfig)
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>() /*_interactionServiceConfig*/))
            .AddSingleton(new CommandService(_commandConfig))
            .AddSingleton<InteractionHandler>()
            .AddSingleton<TextCommandHandler>()
            .AddLavalink()
            .BuildServiceProvider();

        var client = _services.GetRequiredService<DiscordSocketClient>();

        client.Log += LogAsync;

        // Here we can initialize the service that will register and execute our commands
        await _services.GetRequiredService<InteractionHandler>()
            .InitializeAsync();

        await _services.GetRequiredService<IAudioService>().StartAsync();

        await _services.GetRequiredService<TextCommandHandler>()
            .InstallCommandsAsync();

        // Bot token can be provided from the Configuration object we set up earlier
        await client.LoginAsync(TokenType.Bot, _configuration["token"]);
        await client.StartAsync();

        // Never quit the program until manually forced to.
        await Task.Delay(Timeout.Infinite);
    }

    private static Task LogAsync(LogMessage message)
    {
        Console.WriteLine(message.ToString());
        return Task.CompletedTask;
    }
}
/*
var builder = new HostApplicationBuilder(args);

// Discord
builder.Services.AddSingleton<DiscordSocketClient>();
builder.Services.AddSingleton<InteractionService>();
builder.Services.AddHostedService<DiscordClientHost>();

// Lavalink
builder.Services.AddLavalink();
builder.Services.AddLogging(x => x.AddConsole().SetMinimumLevel(LogLevel.Trace));

builder.Build().Run();*/