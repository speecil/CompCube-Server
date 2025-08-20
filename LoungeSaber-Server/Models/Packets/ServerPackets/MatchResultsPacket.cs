using LoungeSaber_Server.Models.Client;
using LoungeSaber_Server.Models.ClientData;
using LoungeSaber_Server.Models.Match;
using LoungeSaber_Server.Models.Packets.UserPackets;
using Newtonsoft.Json;

namespace LoungeSaber_Server.Models.Packets.ServerPackets;


public class MatchResultsPacket : ServerPacket
{
    public override ServerPacketTypes PacketType => ServerPacketTypes.MatchResults;

    [JsonProperty("winningScore")] public readonly MatchScore WinnerScore;

    [JsonProperty("losingScore")] public readonly MatchScore LoserScore;

    [JsonProperty("mmrChange")] public readonly int MmrChange;

    [JsonConstructor]
    public MatchResultsPacket(MatchScore winner, MatchScore loser, int mmrChange)
    {
        WinnerScore = winner;
        LoserScore = loser;
        MmrChange = mmrChange;
    }

    public MatchResultsPacket(MatchResultsData matchResultsData)
    {
        WinnerScore = matchResultsData.Winner;
        LoserScore = matchResultsData.Loser;
        MmrChange = matchResultsData.MmrChange;
    }
}