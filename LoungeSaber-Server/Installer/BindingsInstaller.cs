using LoungeSaber_Server.Api.Controllers;
using LoungeSaber_Server.BeatSaverApi;
using LoungeSaber_Server.Discord;
using LoungeSaber_Server.Discord.Commands;
using LoungeSaber_Server.Discord.Events;
using LoungeSaber_Server.Gameplay.Matchmaking;
using LoungeSaber_Server.Interfaces;
using LoungeSaber_Server.Models.Server;
using LoungeSaber_Server.SQL;

namespace LoungeSaber_Server.Installer;

public static class BindingsInstaller
{
    public static void InstallBindings(IServiceCollection services)
    {
        services.AddSingleton<MatchLog>();
        services.AddSingleton<MapData>();
        services.AddSingleton<UserData>();

        services.AddSingleton<ServerStatusManager>();
        services.AddSingleton<ConnectionManager>();

        if (Program.Debug)
            services.AddSingleton<IMatchmaker, DebugMatchmaker>();
        else 
            services.AddSingleton<IMatchmaker, Matchmaker>();
        
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