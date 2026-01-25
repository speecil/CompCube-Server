using System.Reflection;
using CompCube_Server.Api.BeatSaver;
using CompCube_Server.Api.Controllers;
using CompCube_Server.Discord;
using CompCube_Server.Discord.Commands;
using CompCube_Server.Discord.Events;
using CompCube_Server.Gameplay.Events;
using CompCube_Server.Gameplay.Match;
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
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        InstallBindings(builder.Services);
        
        builder.Services.AddDiscordGateway((options, service) =>
        {
            IConfiguration configuration = service.GetRequiredService<IConfiguration>();
            options.Token = configuration.GetSection("Discord").GetValue<string>("Token");
        }).AddApplicationCommands();

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
        
        host.Services.GetRequiredService<ConnectionManager>();
        
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

        services.AddSingleton<GameMatchFactory>();
        services.AddSingleton<EventFactory>();
        
        services.AddSingleton<QueueManager>();
        services.AddSingleton<EventsManager>();
        
        services.AddSingleton<IQueue, DebugQueue>();
        services.AddSingleton<IQueue, StandardCasualQueue>();
        services.AddSingleton<IQueue, StandardCompetitiveQueue>();

        services.AddSingleton<BeatSaverApiWrapper>();

        services.AddSingleton<LeaderboardApiController>();
        services.AddSingleton<MapApiController>();
        services.AddSingleton<ServerStatusApiController>();
        services.AddSingleton<UserApiController>();
        services.AddSingleton<EventApiController>();
        
        services.AddSingleton<MatchInfoMessageFormatter>();
        services.AddSingleton<UserCommands>();
        services.AddSingleton<ServerCommands>();
        services.AddSingleton<MatchCommands>();
        
        services.AddSingleton<MatchMessageManager>();
        services.AddSingleton<EventMessageManager>();
    }
}