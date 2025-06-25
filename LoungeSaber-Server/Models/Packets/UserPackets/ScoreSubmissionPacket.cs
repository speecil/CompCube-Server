namespace LoungeSaber_Server.Models.Packets.UserPackets;

public class ScoreSubmissionPacket : UserPacket
{
    public override UserPacketTypes PacketType => UserPacketTypes.ScoreSubmission;

    public ScoreSubmissionPacket(int score, bool proMode, int missCount)
    {
        
    }
}