using LoungeSaber_Server.Models.Packets.ServerPackets;
using LoungeSaber_Server.Models.Packets.UserPackets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LoungeSaber_Server.Models.Packets;

public abstract class ServerPacket : Packet
{
    [JsonProperty("type")]
    public abstract ServerPacket.ServerPacketTypes PacketType { get; }
    
    public static ServerPacket Deserialize(string json)
    {
        var jobj = JObject.Parse(json);
        
        if (!jobj.TryGetValue("type", out var packetTypeJToken))
            throw new Exception("Could not deserialize packet!");
        
        if (!Enum.TryParse<ServerPacket.ServerPacketTypes>(packetTypeJToken.ToObject<string>(), out var userPacketType))
            throw new Exception("Could not deserialize packet type!");

        return userPacketType switch
        {
            ServerPacketTypes.JoinResponse => JsonConvert.DeserializeObject<JoinResponse>(json)!,
            _ => throw new Exception("Could not get packet type!")
        };
    }
    
    public enum ServerPacketTypes
    {
        JoinResponse,
        MatchCreated,
        OpponentVoted,
        MatchStarted
    }
}