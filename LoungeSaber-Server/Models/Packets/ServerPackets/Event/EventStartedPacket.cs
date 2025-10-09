using Newtonsoft.Json;

namespace LoungeSaber_Server.Models.Packets.ServerPackets.Event;

[method: JsonConstructor]
public class EventStartedPacket() : ServerPacket
{
    public override ServerPacketTypes PacketType => ServerPacketTypes.EventStarted;
}