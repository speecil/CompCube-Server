using LoungeSaber_Server.Models.Client;
using LoungeSaber_Server.Models.ClientData;
using LoungeSaber_Server.Models.Packets.UserPackets;
using Newtonsoft.Json;

namespace LoungeSaber_Server.Models.Packets.ServerPackets;

public class MatchResultsPacket : ServerPacket
{
    public override ServerPacketTypes PacketType => ServerPacketTypes.MatchResults;
    
    [JsonProperty("opponentScore")]
    public readonly ScoreSubmissionPacket OpponentScore;
        
    [JsonProperty("yourScore")]
    public readonly ScoreSubmissionPacket YourScore;

    [JsonProperty("winner")]
    public readonly MatchWinner Winner;
    
    [JsonProperty("mmrChange")]
    public readonly int MMRChange;

    [JsonProperty("newOpponentUserInfo")] 
    public readonly UserInfo NewOpponentUserInfo;
        
    [JsonProperty("newClientUserInfo")]
    public readonly UserInfo NewClientUserInfo;

    [JsonConstructor]
    public MatchResultsPacket(ScoreSubmissionPacket opponentScore, ScoreSubmissionPacket yourScore, MatchWinner winner, int mmrChange, UserInfo newOpponentUserInfo, UserInfo newClientUserInfo)
    {
        OpponentScore = opponentScore;
        YourScore = yourScore;
        Winner = winner;
        MMRChange = mmrChange;
        NewOpponentUserInfo = newOpponentUserInfo;
        NewClientUserInfo = newClientUserInfo;
    }

    public enum MatchWinner
    {
        You,
        Opponent
    }
}