using Newtonsoft.Json;

namespace LoungeSaber_Server.Models.ClientData;

[method: JsonConstructor]
public class UserInfo(string username, string userId, int mmr, Badge? badge, long rank, string? discordId)
{
    [JsonProperty("username")]
    public string Username { get; private set; } = username;

    [JsonProperty("userId")]
    public string UserId { get; private set; } = userId;

    [JsonProperty("mmr")]
    public int Mmr { get; private set; } = mmr;

    [JsonProperty("badge")]
    public Badge? Badge { get; private set; } = badge;

    [JsonProperty("rank")]
    public long Rank { get; private set; } = rank;

    [JsonProperty("discordId")]
    public string? DiscordId { get; private set; } = discordId;
}