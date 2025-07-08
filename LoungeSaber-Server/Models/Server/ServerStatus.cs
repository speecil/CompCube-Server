using LoungeSaber_Server.ServerState;

namespace LoungeSaber_Server.Models.Server;

public class ServerStatus
{
    public readonly string[] AllowedGameVersions;
    public readonly string[] AllowedModVersions;
    public readonly ServerStateController.ServerState State;

    public ServerStatus(string[] allowedGameVersions, string[] allowedModVersions, ServerStateController.ServerState state)
    {
        AllowedGameVersions = allowedGameVersions;
        AllowedModVersions = allowedModVersions;
        State = state;
    }
}