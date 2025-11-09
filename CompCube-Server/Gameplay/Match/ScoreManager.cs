using CompCube_Models.Models.Match;
using CompCube_Models.Models.Packets.UserPackets;
using CompCube_Server.Interfaces;

namespace CompCube_Server.Gameplay.Match;

public class ScoreManager
{
    public event Action<MatchScore, MatchScore>? OnWinnerDetermined;

    private readonly List<MatchScore> _matchScores = [];
    
    public ScoreManager(IConnectedClient playerOne, IConnectedClient playerTwo)
    {
        playerOne.OnScoreSubmission += OnScoreSubmitted;
        playerTwo.OnScoreSubmission += OnScoreSubmitted;
    }

    private void OnScoreSubmitted(ScoreSubmissionPacket scorePacket, IConnectedClient client)
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