using Newtonsoft.Json;

namespace LoungeSaber_Server.Models.Packets.UserPackets;

public class JoinRequestPacket : UserPacket
{
    [JsonProperty("username")]
    public string UserName { get; private set; }
    
    [JsonProperty("userId")]
    public string UserId { get; private set; }

    [JsonConstructor]
    public JoinRequestPacket(string userName, string userId)
    {
        UserName = userName;
        UserId = userId;
    }

    public override UserPacketTypes PacketType =>  UserPacketTypes.JoinRequest;
}