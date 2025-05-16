

using Newtonsoft.Json;

namespace LoungeSaber_Server.Models.Maps;

public class VotingMap(string hash, MapDifficulty difficulty)
{
    [JsonProperty("hash")] public string Hash { get; set; } = hash;

    [JsonProperty("characteristic")] public string Characteristic { get; set; } = difficulty.Characteristic;

    [JsonProperty("difficulty")] public string Difficulty { get; set; } = difficulty.Difficulty;

    [JsonProperty("category")] public MapDifficulty.MapCategory Category { get; set; } = difficulty.Category;
}