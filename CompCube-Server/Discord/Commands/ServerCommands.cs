using CompCube_Models.Models.Server;
using CompCube_Server.Discord.Events;
using CompCube_Server.Networking.ServerStatus;
using NetCord;
using NetCord.Gateway;
using NetCord.Services.ApplicationCommands;

namespace CompCube_Server.Discord.Commands;

public class ServerCommands(ServerStatusManager serverStatusManager, MatchMessageManager matchMessageManager, GatewayClient client) : ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand("openserver", "yeah", DefaultGuildUserPermissions = Permissions.Administrator, Contexts = [InteractionContextType.Guild])]
    public async Task<string> OpenServer()
    {
        serverStatusManager.State = ServerState.State.Online;

        return "Set the server state to online!";
    }
}