using Newtonsoft.Json;

namespace LoungeSaber_Server.Models.Client;

public class UserInfo
{
    [JsonProperty("username")]
    public string Username { get; private set; }
    
    [JsonProperty("userId")]
    public string UserId { get; private set; }
    
    [JsonProperty("mmr")]
    public int Mmr { get; private set; }
    
    [JsonProperty("badge")]
    public Badge.Badge? Badge { get; private set; }
    
    [JsonProperty("rank")]
    public long Rank { get; private set; }
    
    [JsonConstructor]
    public UserInfo(string username, string userId, int mmr, Badge.Badge? badge, long rank)
    {
        Username = username;
        UserId = userId;
        Mmr = mmr;
        Badge = badge;
        Rank = rank;
    }
}