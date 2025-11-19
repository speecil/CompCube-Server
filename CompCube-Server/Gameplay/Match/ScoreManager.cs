using CompCube_Models.Models.Match;
using CompCube_Models.Models.Packets.UserPackets;
using CompCube_Server.Interfaces;
using CompCube_Server.Logging;

namespace CompCube_Server.Gameplay.Match;

public class ScoreManager
{
    public event Action<MatchScore, MatchScore>? OnWinnerDetermined;

    private readonly List<MatchScore> _matchScores = [];

    private readonly Logger _logger;
    
    public ScoreManager(IConnectedClient playerOne, IConnectedClient playerTwo, Logger logger)
    {
        playerOne.OnScoreSubmission += OnScoreSubmitted;
        playerTwo.OnScoreSubmission += OnScoreSubmitted;

        _logger = logger;
    }

    private void OnScoreSubmitted(ScoreSubmissionPacket scorePacket, IConnectedClient client)
    {
        _logger.Info("score submitted");
        
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