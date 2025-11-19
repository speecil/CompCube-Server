using CompCube_Models.Models.Server;
using CompCube_Server.Networking.ServerStatus;
using NetCord;
using NetCord.Services.ApplicationCommands;

namespace CompCube_Server.Discord.Commands;

public class ServerCommands(ServerStatusManager serverStatusManager) : ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand("openserver", "yeah", DefaultGuildUserPermissions = Permissions.Administrator, Contexts = [InteractionContextType.Guild])]
    public string OpenServer()
    {
        serverStatusManager.State = ServerState.State.Online;

        return "Set the server state to online!";
    }
}