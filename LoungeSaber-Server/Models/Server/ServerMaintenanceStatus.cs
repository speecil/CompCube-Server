using LoungeSaber_Server.ServerState;
using Newtonsoft.Json;

namespace LoungeSaber_Server.Models.Server;

public class ServerMaintenanceStatus
{
    [JsonProperty("allowedGameVersions")]
    public readonly string[] AllowedGameVersions;
    
    [JsonProperty("allowedModVersions")]
    public readonly string[] AllowedModVersions;
    
    [JsonProperty("state")]
    public readonly ServerMaintenanceStateController.ServerState State;

    public ServerMaintenanceStatus(string[] allowedGameVersions, string[] allowedModVersions, ServerMaintenanceStateController.ServerState state)
    {
        AllowedGameVersions = allowedGameVersions;
        AllowedModVersions = allowedModVersions;
        State = state;
    }
}