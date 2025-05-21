using LoungeSaber_Server.SQL;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LoungeSaber_Server.Models.Networking;

public class UserPacket
{
    [JsonProperty("PacketType")]
    public readonly ActionType Type;
    public readonly JObject JsonData;

    [JsonIgnore]
    public User User
    {
        get
        {
            if (!JsonData.TryGetValue("userId", out var userId) || !UserData.Instance.TryGetUserById(userId.ToObject<string>()!, out var user) || user == null)
                throw new Exception("Could not find user!");

            return user;
        }
    }
    
    [JsonConstructor]
    private UserPacket(ActionType actionType, JObject data)
    {
        Type = actionType;

        JsonData = data;
    }

    public static UserPacket Parse(string json)
    {
        var deserialized = JsonConvert.DeserializeObject<UserPacket>(json);
        
        if (deserialized == null) 
            throw new Exception("User action could not be deserialized");
        
        return deserialized;
    }
    
    public enum ActionType
    {
        VoteOnMap,
        PostScore,
        Join,
        Leave
    }
}