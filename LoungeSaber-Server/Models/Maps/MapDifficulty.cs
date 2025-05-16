using Newtonsoft.Json;

namespace LoungeSaber_Server.Models.Maps;

public class MapDifficulty
{
    [JsonProperty("characteristic")]
    public string Characteristic { get; private set; }
    
    // ??? why does playlistmanager serialize the difficulty like this
    [JsonProperty("name")]
    public string Difficulty { get; private set; }
    
    [JsonIgnore]
    public MapTypes Category { get; private set; }
    
    public MapDifficulty(string characteristic, string difficulty, MapTypes category)
    {
        Characteristic = characteristic;
        Difficulty = difficulty;
        Category = category;
    }

    public enum MapTypes
    {
        Acc,
        Speed,
        Tech,
        Midspeed,
        Balanced,
        Extreme,
        Unknown
    }
}