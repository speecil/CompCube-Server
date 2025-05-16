using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LoungeSaber_Server.Models.Maps;

public class PlaylistMap(string hash, MapDifficulty[] difficulties)
{
    [JsonProperty("hash")] 
    public string Hash { get; set; } = hash;

    [JsonProperty("difficulties")] 
    public MapDifficulty[] Difficulties { get; set; } = difficulties;

    public JObject Serialize() => JObject.FromObject(this);
}