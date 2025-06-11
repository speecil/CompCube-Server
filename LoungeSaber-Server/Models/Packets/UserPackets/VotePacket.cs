namespace LoungeSaber_Server.Models.Packets.UserPackets;

public class VotePacket : UserPacket
{
    public override UserPacketTypes PacketType => UserPacketTypes.Vote;

    public readonly int VoteIndex;

    public VotePacket(int voteIndex)
    {
        VoteIndex = voteIndex;
    }
}