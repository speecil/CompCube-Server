using Microsoft.VisualBasic.CompilerServices;
using Newtonsoft.Json;

namespace LoungeSaber_Server.Models.ClientData;

[method: JsonConstructor]
public class DivisionInfo(DivisionInfo.DivisionName division, byte subDivision, string colorCode)
{
    [JsonProperty("division")] public readonly DivisionName Division = division;
    [JsonProperty("subDivision")] public readonly byte SubDivision = subDivision;
    [JsonProperty("color")] public readonly string Color = colorCode;

    public static DivisionInfo GetDivisionFromMmr(int mmr)
    {
        if (IsInRangeOfValue(0, mmr, 999))
            return new DivisionInfo(DivisionName.Iron, GetSubDivision(mmr, 1000), "#E4E4E6");

        if (IsInRangeOfValue(1000, mmr, 1999))
            return new DivisionInfo(DivisionName.Bronze, GetSubDivision(mmr, 1000), "#CE8946");

        if (IsInRangeOfValue(2000, mmr, 2999))
            return new DivisionInfo(DivisionName.Silver, GetSubDivision(mmr, 1000), "#C4C4C4");

        if (IsInRangeOfValue(3000, mmr, 3999))
            return new DivisionInfo(DivisionName.Gold, GetSubDivision(mmr, 1000), "#EFBF04");

        if (IsInRangeOfValue(4000, mmr, 4999))
            return new DivisionInfo(DivisionName.Diamond, GetSubDivision(mmr, 1000), "#4EE2EC");

        if (IsInRangeOfValue(5000, mmr, 6499))
            return new DivisionInfo(DivisionName.Platinum, GetSubDivision(mmr, 1500), "#D9D9D9");

        if (IsInRangeOfValue(6500, mmr, 7999))
            return new DivisionInfo(DivisionName.Master, GetSubDivision(mmr, 2000), "#950606");

        if (IsInRangeOfValue(8000, mmr, int.MaxValue))
            return new DivisionInfo(DivisionName.GrandMaster, GetSubDivision(mmr, 1), "#950606");

        throw new Exception("Invalid MMR range!");
        
        bool IsInRangeOfValue(int lowerRange, int value, int upperRange) => Math.Clamp(lowerRange, value, upperRange) == value;

        byte GetSubDivision(int mmrValue, int mmrSpan) => (byte) (mmrSpan / mmrValue);
    }

    public enum DivisionName
    {
        Iron,
        Bronze,
        Silver,
        Gold,
        Diamond,
        Platinum,
        Master,
        GrandMaster
    }
}