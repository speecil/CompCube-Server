using Newtonsoft.Json;

namespace LoungeSaber_Server.Models.Packets.UserPackets;

public class VotePacket : UserPacket
{
    public override UserPacketTypes PacketType => UserPacketTypes.Vote;

    [JsonProperty("vote")]
    public readonly int VoteIndex;

    [JsonConstructor]
    public VotePacket(int voteIndex)
    {
        VoteIndex = voteIndex;
    }
}