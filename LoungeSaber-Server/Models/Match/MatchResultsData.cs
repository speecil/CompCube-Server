using LoungeSaber_Server.Models.Client;
using LoungeSaber_Server.Models.ClientData;
using LoungeSaber_Server.Models.Map;
using Newtonsoft.Json;

namespace LoungeSaber_Server.Models.Match;

public record MatchResultsData(MatchScore Winner, MatchScore Loser, int MmrChange, VotingMap? Map, bool Premature, int Id, DateTime Time);

public record MatchScore(UserInfo User, Score? Score);

public record Score(int Points, float RelativeScore, bool ProMode, int Misses, bool FullCombo)
{
    public static Score Empty => new Score(0, 0f, false, 0, true);
    
    public string Serialize() => JsonConvert.SerializeObject(this);

    public static Score? Deserialize(string? json)
    {
        return json == null ? null : JsonConvert.DeserializeObject<Score>(json);
    }
}