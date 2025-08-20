using LoungeSaber_Server.Models.Client;
using LoungeSaber_Server.Models.ClientData;
using LoungeSaber_Server.Models.Map;
using LoungeSaber_Server.Models.Match;
using LoungeSaber_Server.Models.Packets.UserPackets;

namespace LoungeSaber_Server.Gameplay.Match;

public class ScoreManager
{
    private readonly ConnectedClient _playerOne;
    private readonly ConnectedClient _playerTwo;

    public event Action<MatchScore, MatchScore>? OnWinnerDetermined;

    private readonly List<MatchScore> _matchScores = [];
    
    public ScoreManager(ConnectedClient playerOne, ConnectedClient playerTwo)
    {
        _playerOne = playerOne;
        _playerTwo = playerTwo;
        
        playerOne.OnScoreSubmission += OnScoreSubmitted;
        playerTwo.OnScoreSubmission += OnScoreSubmitted;
    }

    private void OnScoreSubmitted(ScoreSubmissionPacket scorePacket, ConnectedClient client)
    {
        client.OnScoreSubmission -= OnScoreSubmitted;

        var matchScore = new MatchScore(client.UserInfo, scorePacket.GetScore());
        
        _matchScores.Add(matchScore);

        if (_matchScores.Count != 2) 
            return;

        var winnerScore = matchScore;
        
        if (_matchScores[0].Score?.Points > scorePacket.Score || (_matchScores[0].Score?.Points == scorePacket.Score && _matchScores[0].User.Mmr >= client.UserInfo.Mmr))
        {
            winnerScore = _matchScores[0];
        }

        var loserScore = winnerScore.User.UserId == client.UserInfo.UserId ? _matchScores[0] : _matchScores[1];
        
        OnWinnerDetermined?.Invoke(winnerScore, loserScore);
    }
}