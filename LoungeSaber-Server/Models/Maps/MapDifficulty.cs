using Newtonsoft.Json;

namespace LoungeSaber_Server.Models.Maps;

public class MapDifficulty(string characteristic, string difficulty, MapDifficulty.MapCategory category)
{
    [JsonProperty("characteristic")]
    public string Characteristic { get; private set; } = characteristic;

    [JsonProperty("name")]
    public string Difficulty { get; private set; } = difficulty;

    [JsonIgnore] public MapCategory Category { get; private set; } = category;
    
    public enum MapCategory
    {
        Speed,
        Midspeed,
        Acc,
        Tech,
        Balanced,
        Extreme,
        Unknown
    }
}