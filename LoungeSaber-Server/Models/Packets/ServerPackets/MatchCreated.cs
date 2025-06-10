using LoungeSaber_Server.Models.Map;
using LoungeSaber_Server.SQL;
using Newtonsoft.Json;

namespace LoungeSaber_Server.Models.Packets.ServerPackets;

public class MatchCreated : ServerPacket
{
    public override ServerPacketTypes PacketType => ServerPacketTypes.MatchCreated;
    
    [JsonProperty("votingOptions")]
    public readonly VotingMap[] Maps;
    
    [JsonProperty("opponent")]
    public readonly UserData Opponent;

    public MatchCreated(VotingMap[] maps, UserData opponent)
    {
        Maps = maps;
        Opponent = opponent;
    }
}