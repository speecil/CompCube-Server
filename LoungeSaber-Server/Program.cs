using LoungeSaber_Server.Api.Controllers;
using LoungeSaber_Server.BeatSaverApi;
using LoungeSaber_Server.Discord;
using LoungeSaber_Server.Discord.Commands;
using LoungeSaber_Server.Discord.Events;
using LoungeSaber_Server.Gameplay.Matchmaking;
using LoungeSaber_Server.Interfaces;
using LoungeSaber_Server.Logging;
using LoungeSaber_Server.Models.Server;
using LoungeSaber_Server.SQL;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;

namespace LoungeSaber_Server;

public class Program
{
    public static bool Debug { get; private set; } = false;
        
    public static async Task Main(string[] args)
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

        await host.RunAsync();
    }
        
    private static void InstallBindings(IServiceCollection services)
    {
        services.AddSingleton<Logger>();
        
        services.AddSingleton<MatchLog>();
        services.AddSingleton<MapData>();
        services.AddSingleton<UserData>();

        services.AddSingleton<ServerStatusManager>();
        services.AddSingleton<ConnectionManager>();

        if (Program.Debug)
            services.AddSingleton<IQueue, DebugQueue>();
        else 
            services.AddSingleton<IQueue, Queue>();
        
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
    }
}