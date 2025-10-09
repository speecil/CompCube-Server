using LoungeSaber_Server.Models.ClientData;
using Newtonsoft.Json;

namespace LoungeSaber_Server.Models.Packets.ServerPackets.Event;

[method: JsonConstructor]
public class EventMatchStartedPacket(MatchStartedPacket matchData, UserInfo opponent) : ServerPacket
{
    public override ServerPacketTypes PacketType => ServerPacketTypes.EventMatchCreated;
    
    [JsonProperty("matchData")]
    public readonly MatchStartedPacket MatchData = matchData;
    
    [JsonProperty("opponent")]
    public readonly UserInfo Opponent = opponent;
}