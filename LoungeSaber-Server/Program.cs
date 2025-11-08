using LoungeSaber_Server.Api.BeatSaver;
using LoungeSaber_Server.Api.Controllers;
using LoungeSaber_Server.Discord;
using LoungeSaber_Server.Discord.Commands;
using LoungeSaber_Server.Discord.Events;
using LoungeSaber_Server.Gameplay.Events;
using LoungeSaber_Server.Gameplay.Matchmaking;
using LoungeSaber_Server.Interfaces;
using LoungeSaber_Server.Logging;
using LoungeSaber_Server.Networking.ServerStatus;
using LoungeSaber_Server.SQL;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;

namespace LoungeSaber_Server;

public class Program
{
    public static bool Debug { get; private set; } = false;
        
    public static void Main(string[] args)
    {
        if (args.Contains("--debug"))
            Debug = true;
            
        var builder = WebApplication.CreateBuilder(args);
        
        InstallBindings(builder.Services);
        
        builder.Services.AddDiscordGateway().AddApplicationCommands();

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
            
        var host = builder.Build();
            
        if (host.Environment.IsDevelopment())
        {
            host.UseSwagger();
            host.UseSwaggerUI();
        }

        host.UseHttpsRedirection();
        host.MapControllers();
            
        host.AddModules(typeof(Program).Assembly);

        host.UseGatewayHandlers();

        host.Run();
    }
        
    private static void InstallBindings(IServiceCollection services)
    {
        services.AddSingleton<Logger>();
        
        services.AddSingleton<MatchLog>();
        services.AddSingleton<MapData>();
        services.AddSingleton<UserData>();

        services.AddSingleton<ServerStatusManager>();
        
        services.AddSingleton<ConnectionManager>();
        
        services.AddSingleton<QueueManager>();
        services.AddSingleton<EventManager>();
        
        services.AddSingleton<IQueue, DebugQueue>();
        services.AddSingleton<IQueue, StandardQueue>();
        
        services.AddSingleton<MatchCompletedMessageManager>();
        services.AddSingleton<MatchInfoMessageFormatter>();
        services.AddSingleton<UserCommands>();
        services.AddSingleton<ServerCommands>();
        services.AddSingleton<MatchCommands>();

        services.AddSingleton<BeatSaverApiWrapper>();

        services.AddSingleton<LeaderboardApiController>();
        services.AddSingleton<MapApiController>();
        services.AddSingleton<ServerStatusApiController>();
        services.AddSingleton<UserApiController>();
        services.AddSingleton<EventApiController>();

        // force instantiation of connectionmanager as non lazy
        // lazy fucks
        services.BuildServiceProvider().GetRequiredService<ConnectionManager>();
    }
}