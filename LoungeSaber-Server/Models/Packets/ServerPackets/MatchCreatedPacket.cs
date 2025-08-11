using LoungeSaber_Server.Models.Client;
using LoungeSaber_Server.Models.ClientData;
using LoungeSaber_Server.Models.Map;
using LoungeSaber_Server.SQL;
using Newtonsoft.Json;

namespace LoungeSaber_Server.Models.Packets.ServerPackets;

public class MatchCreatedPacket : ServerPacket
{
    public override ServerPacketTypes PacketType => ServerPacketTypes.MatchCreated;
    
    [JsonProperty("votingOptions")]
    public readonly VotingMap[] Maps;
    
    [JsonProperty("opponent")]
    public readonly UserInfo Opponent;

    [JsonConstructor]
    public MatchCreatedPacket(VotingMap[] maps, UserInfo opponent)
    {
        Maps = maps;
        Opponent = opponent;
    }
}