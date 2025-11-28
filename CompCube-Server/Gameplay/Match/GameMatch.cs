using CompCube_Models.Models.Match;
using CompCube_Server.Interfaces;

namespace CompCube_Server.Gameplay.Match;

public class GameMatch
{
    private MatchSettings _matchSettings;

    private readonly Dictionary<IConnectedClient, Team> _teams = new();

    private readonly Dictionary<Team, int> _points = new();

    private ScoreManager? _currentRoundScoreManager = null;
    
    public void Init(IConnectedClient[] red, IConnectedClient[] blue, MatchSettings settings)
    {
        _matchSettings = settings;
        
        SetupPlayer(red, Team.Red);
        SetupPlayer(blue, Team.Blue);
        
        _points.Add(Team.Red, 0);
        _points.Add(Team.Blue, 0);
        
        return;
        
        void SetupPlayer(IConnectedClient[] players, Team team)
        {
            foreach (var player in players)
            {
                player.OnDisconnected += HandleClientDisconnect;
                
                _teams.Add(player, team);
            }
        }
    }

    public async Task StartMatchAsync()
    {
        _currentRoundScoreManager = new ScoreManager(_teams);
        
        _currentRoundScoreManager.OnScoresSubmitted += HandleScores;
    }

    private void HandleScores(Dictionary<IConnectedClient, Score> scores)
    {
        var redPoints = scores.Where(i => _teams[i.Key] == Team.Red).Sum(i => i.Value.Points);
        var bluePoints = scores.Where(i => _teams[i.Key] == Team.Blue).Sum(i => i.Value.Points);

        if (redPoints >= bluePoints)
            _points[Team.Red] += 1;

        if (bluePoints >= redPoints)
            _points[Team.Blue] += 1;
        
    }

    public void StartMatch() => StartMatchAsync();

    private void HandleClientDisconnect(IConnectedClient client)
    {
        client.OnDisconnected -= HandleClientDisconnect;

        _teams.Remove(client);
    }

    public enum Team
    {
        Red,
        Blue
    }
}