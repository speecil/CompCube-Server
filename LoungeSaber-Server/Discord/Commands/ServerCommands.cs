using CompCube_Models.Models.Server;
using LoungeSaber_Server.Discord.Events;
using LoungeSaber_Server.Networking.ServerStatus;
using NetCord;
using NetCord.Services.ApplicationCommands;

namespace LoungeSaber_Server.Discord.Commands;

public class ServerCommands(ServerStatusManager serverStatusManager, MatchCompletedMessageManager matchCompletedMessageManager) : ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand("openserver", "yeah", DefaultGuildUserPermissions = Permissions.Administrator, Contexts = [InteractionContextType.Guild])]
    public async Task<string> OpenServer()
    {
        serverStatusManager.State = ServerState.State.Online;

        var channel = await Context.Client.Rest.GetChannelAsync(1400279911008174282) as TextChannel;

        if (channel == null)
            return "Could not find the match logging channel!";
        
        matchCompletedMessageManager.Init(channel);

        return "Set the server state to online!";
    }
}