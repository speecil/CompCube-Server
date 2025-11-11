using CompCube_Server.Api.BeatSaver;
using CompCube_Server.Api.Controllers;
using CompCube_Server.Discord;
using CompCube_Server.Discord.Commands;
using CompCube_Server.Discord.Events;
using CompCube_Server.Gameplay.Events;
using CompCube_Server.Gameplay.Matchmaking;
using CompCube_Server.Interfaces;
using CompCube_Server.Logging;
using CompCube_Server.Networking.ServerStatus;
using CompCube_Server.SQL;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;

namespace CompCube_Server;

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
        services.AddSingleton<EventsManager>();
        
        services.AddSingleton<IQueue, DebugQueue>();
        services.AddSingleton<IQueue, StandardQueue>();
        
        services.AddSingleton<MatchMessageManager>();
        services.AddSingleton<EventMessageManager>();
        
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