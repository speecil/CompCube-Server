using LoungeSaber_Server.ServerState;
using Newtonsoft.Json;

namespace LoungeSaber_Server.Models.Server;

public class ServerStatus
{
    [JsonProperty("allowedGameVersions")]
    public readonly string[] AllowedGameVersions;
    
    [JsonProperty("allowedModVersions")]
    public readonly string[] AllowedModVersions;
    
    [JsonProperty("state")]
    public readonly ServerStateController.ServerState State;

    public ServerStatus(string[] allowedGameVersions, string[] allowedModVersions, ServerStateController.ServerState state)
    {
        AllowedGameVersions = allowedGameVersions;
        AllowedModVersions = allowedModVersions;
        State = state;
    }
}