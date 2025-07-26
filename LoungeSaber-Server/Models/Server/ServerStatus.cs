using LoungeSaber_Server.ServerMaintenanceState;
using Newtonsoft.Json;

namespace LoungeSaber_Server.Models.Server;

public class ServerStatus
{
    [JsonProperty("allowedGameVersions")]
    public readonly string[] AllowedGameVersions;
    
    [JsonProperty("allowedModVersions")]
    public readonly string[] AllowedModVersions;
    
    [JsonProperty("state")]
    public readonly ServerMaintenanceStateController.ServerState State;

    public ServerStatus(string[] allowedGameVersions, string[] allowedModVersions, ServerMaintenanceStateController.ServerState state)
    {
        AllowedGameVersions = allowedGameVersions;
        AllowedModVersions = allowedModVersions;
        State = state;
    }

    public static ServerStatus GetServerMaintenanceState() => new(["1.39.1", "1.40.8"], ["1.0.0"], ServerMaintenanceStateController.State);
}