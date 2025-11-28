using CompCube_Models.Models.Match;
using CompCube_Models.Models.Packets.UserPackets;
using CompCube_Server.Interfaces;

namespace CompCube_Server.Gameplay.Match;

public class ScoreManager
{
    private readonly Dictionary<IConnectedClient, Score?> _scores;

    public event Action<Dictionary<IConnectedClient, Score>>? OnScoresSubmitted;
    
    public ScoreManager(Dictionary<IConnectedClient, GameMatch.Team> players)
    {
        _scores = players.Select(i => new KeyValuePair<IConnectedClient, Score?>(i.Key, null)).ToDictionary();

        foreach (var player in players)
        {
            player.Key.OnScoreSubmission += HandleScoreSubmitted;
        }
    }

    private void HandleScoreSubmitted(ScoreSubmissionPacket score, IConnectedClient client)
    {
        client.OnScoreSubmission -= HandleScoreSubmitted;
        
        _scores[client] = score.GetScore();
        
        CheckIfAllScoresAreSubmitted();
    }

    private void CheckIfAllScoresAreSubmitted()
    {
        if (_scores.Any(i => i.Value == null))
            return;
        
        OnScoresSubmitted?.Invoke(_scores.Select(i => new KeyValuePair<IConnectedClient,Score>(i.Key, i.Value ?? Score.Empty)).ToDictionary());
    }

    public void HandlePlayerDisconneced(IConnectedClient player)
    {
        player.OnScoreSubmission -= HandleScoreSubmitted;
        
        CheckIfAllScoresAreSubmitted();
    }
}