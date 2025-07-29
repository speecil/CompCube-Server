using NetCord;
using NetCord.Gateway;
using NetCord.Logging;

namespace LoungeSaber_Server.Discord;

public class DiscordBot
{
    public static void RunDiscordBot()
    {
        GatewayClient client = new(new BotToken("MTM5OTU4OTYyODU5MTIxNDU5Mw.G8Fc1T.c5bnh5-jul0-BQu-c1QGIJOgr92ifur1fdeJOE"), new GatewayClientConfiguration
        {
            Logger = new ConsoleLogger(),
        });

        Task.Run(async () =>
        {
            await client.StartAsync();
            await Task.Delay(-1);
        });
    }
}