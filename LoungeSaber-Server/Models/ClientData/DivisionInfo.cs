using Microsoft.OpenApi.Extensions;
using Microsoft.VisualBasic.CompilerServices;
using Newtonsoft.Json;

namespace LoungeSaber_Server.Models.ClientData;

[method: JsonConstructor]
public class DivisionInfo(DivisionInfo.DivisionName division, int subDivision, string colorCode, bool glow)
{
    [JsonProperty("division")] public string Division { get; private set; } = division.ToString();
    [JsonProperty("subDivision")] public int SubDivision { get; private set; } = subDivision;
    [JsonProperty("color")] public string Color { get; private set; } = colorCode;
    [JsonProperty("glow")] public bool Glow { get; private set; } = glow;

    public static DivisionInfo GetDivisionFromMmr(int mmr)
    {
        if (IsInRangeOfValue(0, mmr, 999))
            return new DivisionInfo(DivisionName.Iron, GetSubDivision(mmr, 1000), "#E4E4E6", false);
        if (IsInRangeOfValue(1000, mmr, 1999))
            return new DivisionInfo(DivisionName.Bronze, GetSubDivision(mmr - 1000, 1000), "#CE8946", false);

        if (IsInRangeOfValue(2000, mmr, 2999))
            return new DivisionInfo(DivisionName.Silver, GetSubDivision(mmr - 2000, 1000), "#C4C4C4", false);

        if (IsInRangeOfValue(3000, mmr, 3999))
            return new DivisionInfo(DivisionName.Gold, GetSubDivision(mmr - 3000, 1000), "#EFBF04", false);

        if (IsInRangeOfValue(4000, mmr, 4999))
            return new DivisionInfo(DivisionName.Platinum, GetSubDivision(mmr - 5000, 1000), "#D9D9D9", false);

        if (IsInRangeOfValue(5000, mmr, 6499))
            return new DivisionInfo(DivisionName.Diamond, GetSubDivision(mmr - 4000, 1500), "#4EE2EC", false);

        if (IsInRangeOfValue(6500, mmr, 7999))
            return new DivisionInfo(DivisionName.Emerald, GetSubDivision(mmr - 6500, 1500), "#50C878", false);

        if (IsInRangeOfValue(8000, mmr, 9999))
            return new DivisionInfo(DivisionName.Master, GetSubDivision(mmr - 8000, 2000), "#950606", false);
        
        if (IsInRangeOfValue(10000, mmr, int.MaxValue))
            return new DivisionInfo(DivisionName.GrandMaster, 1, "#950606", true);
        
        throw new Exception("Invalid MMR range!");
    }
    
    private static int GetSubDivision(int mmrValue, int mmrSpan)
    {
        if (mmrValue == 0 || mmrSpan == 0)
            return 1;

        var value = (int) ((double) mmrValue / mmrSpan * 4) + 1;
            
        return Math.Clamp(value, 1, 4);
    }
    
    private static bool IsInRangeOfValue(int lowerRange, int value, int upperRange) => Math.Clamp(value, lowerRange, upperRange) == value;

    public enum DivisionName
    {
        Iron,
        Bronze,
        Silver,
        Gold,
        Platinum,
        Diamond,
        Emerald,
        Master,
        GrandMaster
    }
}