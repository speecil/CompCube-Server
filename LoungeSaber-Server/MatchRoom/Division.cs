using System.Drawing;
using LoungeSaber_Server.Models;
using LoungeSaber_Server.Models.Maps;
using LoungeSaber_Server.SQL;
using Newtonsoft.Json;

namespace LoungeSaber_Server.SkillDivision;

public class Division
{
    public int MinMMR { get; private set; }
    public int MaxMMR { get; private set; }

    public string DivisionName { get; private set; }
    public Color DivisionColor { get; private set; }
    
    public MapDifficulty.MapCategory[] DisallowedMapTypes { get; private set; }
    

    [JsonIgnore]
    public readonly MatchRoom.MatchLobby DivisionLobby;

    // TODO: make constructor private
    public Division(int minMMR, int maxMMR, string divisionName, Color divisionColor, MapDifficulty.MapCategory[] disallowedMapTypes)
    {
        MinMMR = minMMR;
        MaxMMR = maxMMR;
        DivisionName = divisionName;
        DivisionColor = divisionColor;
        DisallowedMapTypes = disallowedMapTypes;

        DivisionLobby = new MatchRoom.MatchLobby(this);
    }

    public List<VotingMap> GetRandomMaps(int amount)
    {
        var votingMaps = new List<VotingMap>();
        
        var random = new Random();

        var allMapDifficulties = MapData.Instance.GetAllIndividualDifficulties();

        while (votingMaps.Count < amount)
        {
            var map = allMapDifficulties[random.Next(allMapDifficulties.Count + 1)];
            
            if (votingMaps.Any(i => i.Hash == map.Hash)) continue;
            if (DisallowedMapTypes.Contains(map.Category)) continue;
            
            votingMaps.Add(map);
        }

        return votingMaps;
    }

    public static Division Parse(string json)
    {
        var division = JsonConvert.DeserializeObject<Division>(json);
        
        if (division == null) 
            throw new Exception("Could not deserialize division from config!");
        
        return division;
    }
}

public struct Color(int r, int g, int b)
{
    public int r { get; set; } = r;
    public int g { get; set; } = g;
    public int b { get; set; } = b;
}