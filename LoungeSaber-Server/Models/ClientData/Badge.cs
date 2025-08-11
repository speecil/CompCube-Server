using Newtonsoft.Json;

namespace LoungeSaber_Server.Models.ClientData;

[method: JsonConstructor]
public class Badge(string name, string colorCode, bool bold)
{
    [JsonProperty("badgeName")]
    public string Name { get; private set; } = name;

    [JsonProperty("badgeColor")]
    public string ColorCode { get; private set; } = colorCode;

    [JsonProperty("badgeBold")]
    public bool Bold { get; private set; } = bold;
}