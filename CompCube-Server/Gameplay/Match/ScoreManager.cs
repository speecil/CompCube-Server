using CompCube_Models.Models.Match;
using CompCube_Models.Models.Packets.UserPackets;
using CompCube_Server.Interfaces;

namespace CompCube_Server.Gameplay.Match;

public class ScoreManager : IDisposable
{
    private readonly Dictionary<IConnectedClient, Score?> _scores;

    private readonly Action<Dictionary<IConnectedClient, Score>> _onScoresDecidedCallback;
    
    public ScoreManager(IConnectedClient[] players, Action<Dictionary<IConnectedClient, Score>> onScoresDecidedCallback)
    {
        _onScoresDecidedCallback = onScoresDecidedCallback;
        
        _scores = players.Select(i => new KeyValuePair<IConnectedClient, Score?>(i, null)).ToDictionary();

        foreach (var player in players)
        {
            player.OnScoreSubmission += HandleScoreSubmitted;
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

        var scoreDictionary = _scores
            .Select(i => new KeyValuePair<IConnectedClient, Score>(i.Key, i.Value ?? Score.Empty))
            .OrderBy(i => i.Value.Points).ToDictionary();
        
        _onScoresDecidedCallback.Invoke(scoreDictionary);
    }

    public void HandlePlayerDisconneced(IConnectedClient player)
    {
        player.OnScoreSubmission -= HandleScoreSubmitted;
        
        CheckIfAllScoresAreSubmitted();
    }

    public void Dispose() => _scores.Keys.ToList().ForEach(i => i.OnScoreSubmission -= HandleScoreSubmitted);
}