using Newtonsoft.Json;

namespace LoungeSaber_Server.Models.Packets.ServerPackets;

public class PrematureMatchEnd : ServerPacket
{
    public override ServerPacketTypes PacketType => ServerPacketTypes.PrematureMatchEnd;
    
    [JsonProperty("reason")]
    public readonly string Reason;

    [JsonConstructor]
    public PrematureMatchEnd(string reason)
    {
        Reason = reason;
    }
}