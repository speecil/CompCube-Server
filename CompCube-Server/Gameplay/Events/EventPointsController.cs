using CompCube_Models.Models.Match;
using CompCube_Models.Models.Packets.UserPackets;
using CompCube_Server.Interfaces;

namespace CompCube_Server.Gameplay.Events;

public class EventPointsController
{
    private readonly Dictionary<IConnectedClient, int> _clientPoints = new();
    
    private readonly List<MatchScore> _mostRecentMatchScores = new();
    
    public event Action<Dictionary<IConnectedClient, int>>? OnPointsUpdated;
    public event Action<List<MatchScore>, List<IConnectedClient>>? OnScoresUpdated; 
    
    public EventPointsController(List<IConnectedClient> clients)
    {
        clients.ForEach(i =>
        {
            _clientPoints.Add(i, 0);
            
            i.OnScoreSubmission += OnScoreSubmission;
        });
    }

    private void OnScoreSubmission(ScoreSubmissionPacket score, IConnectedClient client)
    {
        _mostRecentMatchScores.Add(new MatchScore(client.UserInfo, score.GetScore()));

        var clientsWithUnsubmittedScores = _clientPoints.Keys.Where(i => _mostRecentMatchScores.Any(j => j.User.UserId == i.UserInfo.UserId)).ToList();
        
        OnScoresUpdated?.Invoke(_mostRecentMatchScores, clientsWithUnsubmittedScores);
    }

    public void ComputeScores()
    {
        //TODO: implement
        
        OnPointsUpdated?.Invoke(_clientPoints);
    }
}