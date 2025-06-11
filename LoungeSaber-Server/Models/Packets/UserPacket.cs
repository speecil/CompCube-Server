using LoungeSaber_Server.Models.Packets.UserPackets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LoungeSaber_Server.Models.Packets;

public abstract class UserPacket : Packet
{
    [JsonProperty("type")]
    public abstract UserPacketTypes PacketType { get; }
    
    public static UserPacket Deserialize(string json)
    {
        var jobj = JObject.Parse(json);
        
        if (!jobj.TryGetValue("type", out var packetTypeJToken))
            throw new Exception("Could not deserialize packet!");
        
        if (!Enum.TryParse<UserPacketTypes>(packetTypeJToken.ToObject<string>(), out var userPacketType))
            throw new Exception("Could not deserialize packet type!");

        return userPacketType switch
        {
            UserPacketTypes.JoinRequest => JsonConvert.DeserializeObject<JoinRequestPacket>(json)!,
            _ => throw new Exception("Could not get packet type!")
        };
    }

    public enum UserPacketTypes
    {
        JoinRequest,
        Vote
    }
}