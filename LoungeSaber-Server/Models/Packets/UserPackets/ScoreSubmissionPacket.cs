using LoungeSaber_Server.Models.Match;
using Newtonsoft.Json;

namespace LoungeSaber_Server.Models.Packets.UserPackets;

public class ScoreSubmissionPacket : UserPacket
{
    public override UserPacketTypes PacketType => UserPacketTypes.ScoreSubmission;

    [JsonProperty("score")]
    public readonly int Score;
        
    [JsonProperty("maxScore")]
    public readonly int MaxScore;
    
    [JsonProperty("proMode")]
    public readonly bool ProMode;
    
    [JsonProperty("missCount")]
    public readonly int MissCount;
        
    [JsonProperty("fullCombo")]
    public readonly bool FullCombo;
    
    [JsonConstructor]
    public ScoreSubmissionPacket(int score, int maxScore, bool proMode, int missCount, bool fullCombo)
    {
        Score = score;
        MaxScore = maxScore;
        ProMode = proMode;
        MissCount = missCount;
        FullCombo = fullCombo;
    }

    public Score GetScore() => new Score(Score, (float) Score / MaxScore, ProMode, MissCount, FullCombo);
}