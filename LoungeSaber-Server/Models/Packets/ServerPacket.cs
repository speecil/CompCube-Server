using LoungeSaber_Server.Models.Packets.ServerPackets;
using LoungeSaber_Server.Models.Packets.UserPackets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LoungeSaber_Server.Models.Packets;

public abstract class ServerPacket : Packet
{
    [JsonProperty("type")]
    public abstract ServerPacketTypes PacketType { get; }
    
    public enum ServerPacketTypes
    {
        JoinResponse,
        MatchCreated,
        OpponentVoted,
        MatchStarted,
        MatchResults,
        PrematureMatchEnd,
        EventStarted,
        EventEnded
    }
}