using Newtonsoft.Json;

namespace LoungeSaber_Server.Models.Packets.UserPackets;

public class ScoreSubmissionPacket : UserPacket
{
    public override UserPacketTypes PacketType => UserPacketTypes.ScoreSubmission;

    [JsonProperty("score")]
    public readonly int Score;
    
    [JsonProperty("proMode")]
    public readonly bool ProMode;
    
    [JsonProperty("missCount")]
    public readonly int MissCount;
    
    [JsonConstructor]
    public ScoreSubmissionPacket(int score, bool proMode, int missCount)
    {
        Score = score;
        ProMode = proMode;
        MissCount = missCount;
    }
}