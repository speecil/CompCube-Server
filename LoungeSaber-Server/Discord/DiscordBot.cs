using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;

namespace LoungeSaber_Server.Discord;

public class DiscordBot
{
    public static void Start()
    {
        var builder = Host.CreateApplicationBuilder();

        builder.Services.AddDiscordGateway().AddApplicationCommands();

        var host = builder.Build();
        
        host.AddModules(typeof(Program).Assembly);

        host.UseGatewayHandlers();

        Task.Run(async () =>
        {
            await host.RunAsync();
        });
    }
}