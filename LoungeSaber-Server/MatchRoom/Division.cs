using System.Drawing;
using Newtonsoft.Json;

namespace LoungeSaber_Server.SkillDivision;

public class Division
{
    public int MinMMR { get; private set; }
    public int MaxMMR { get; private set; }

    public string DivisionName { get; private set; }
    public Color DivisionColor { get; private set; }

    [JsonIgnore]
    public readonly MatchRoom.MatchRoom DivisionRoom;

    // TODO: make constructor private
    public Division(int minMMR, int maxMMR, string divisionName, Color divisionColor)
    {
        MinMMR = minMMR;
        MaxMMR = maxMMR;
        DivisionName = divisionName;
        DivisionColor = divisionColor;

        DivisionRoom = new MatchRoom.MatchRoom(this);
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