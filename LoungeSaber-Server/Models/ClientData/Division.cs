using Newtonsoft.Json;

namespace LoungeSaber_Server.Models.ClientData;

[method: JsonConstructor]
public class Division(string divisionName, string colorCode)
{
    [JsonProperty("name")] public readonly string Name = divisionName;
    [JsonProperty("color")] public readonly string Color = colorCode;
}