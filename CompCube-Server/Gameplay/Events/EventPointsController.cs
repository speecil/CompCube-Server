using CompCube_Models.Models.ClientData;
using CompCube_Models.Models.Match;
using CompCube_Models.Models.Packets.UserPackets;
using CompCube_Server.Interfaces;

namespace CompCube_Server.Gameplay.Events;

public class EventPointsController
{
    private readonly Dictionary<IConnectedClient, int> _clientPoints = new();
    
    private readonly List<MatchScore> _mostRecentMatchScores = new();
    
    public event Action<Dictionary<UserInfo, int>>? OnPointsUpdated;
    public event Action<List<MatchScore>, List<UserInfo>>? OnScoresUpdated; 
    
    public EventPointsController(List<IConnectedClient> clients)
    {
        clients.ForEach(i =>
        {
            _clientPoints.Add(i, 0);
            
            i.OnScoreSubmission += OnScoreSubmission;
            i.OnDisconnected += OnDisconnected;
        });
    }

    private void OnDisconnected(IConnectedClient client)
    {
        client.OnScoreSubmission -= OnScoreSubmission;
        client.OnDisconnected -= OnDisconnected;
        
        _clientPoints.Remove(client);
    }

    private void OnScoreSubmission(ScoreSubmissionPacket score, IConnectedClient client)
    {
        _mostRecentMatchScores.Add(new MatchScore(client.UserInfo, score.GetScore()));

        var clientsWithUnsubmittedScores = _clientPoints.Keys.Where(i => _mostRecentMatchScores.Any(j => j.User.UserId == i.UserInfo.UserId)).ToList();
        
        OnScoresUpdated?.Invoke(_mostRecentMatchScores, clientsWithUnsubmittedScores.Select(i => i.UserInfo).ToList());
    }

    public void ComputeScores()
    {
        var orderedPoints = _clientPoints.OrderByDescending(i => i.Value);
        
        OnPointsUpdated?.Invoke(orderedPoints.Select(i => new KeyValuePair<UserInfo, int>(i.Key.UserInfo, i.Value)).ToDictionary());
        
        _mostRecentMatchScores.Clear();
    }
}