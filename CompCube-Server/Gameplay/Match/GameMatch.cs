using CompCube_Models.Models.Map;
using CompCube_Models.Models.Match;
using CompCube_Models.Models.Packets;
using CompCube_Server.Interfaces;
using CompCube_Server.SQL;

namespace CompCube_Server.Gameplay.Match;

public class GameMatch(MapData mapData)
{
    private MatchSettings _matchSettings;

    private readonly Dictionary<IConnectedClient, Team> _teams = new();

    private readonly Dictionary<Team, int> _points = new();

    private ScoreManager? _currentRoundScoreManager = null;
    private VoteManager? _currentRoundVoteManager = null;
    
    public void Init(IConnectedClient[] red, IConnectedClient[] blue, MatchSettings settings)
    {
        _matchSettings = settings;
        
        SetupTeam(red, Team.Red);
        SetupTeam(blue, Team.Blue);
        
        _points.Add(Team.Red, 0);
        _points.Add(Team.Blue, 0);
        
        return;
        
        void SetupTeam(IConnectedClient[] players, Team team)
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
        _currentRoundVoteManager = new VoteManager(_teams.Keys.ToArray(), mapData, HandleVoteDecided);
        // send match start packets here
        // send round start packets here
        
        
    }

    private void HandleVoteDecided(VotingMap votingMap)
    {
        _currentRoundScoreManager = new ScoreManager(_teams, HandleScores);
    }

    private void HandleScores(Dictionary<IConnectedClient, Score> scores)
    {
        var redPoints = scores.Where(i => _teams[i.Key] == Team.Red).Sum(i => i.Value.Points);
        var bluePoints = scores.Where(i => _teams[i.Key] == Team.Blue).Sum(i => i.Value.Points);

        if (redPoints >= bluePoints)
            _points[Team.Red] += 1;

        if (bluePoints >= redPoints)
            _points[Team.Blue] += 1;

        if (_currentRoundScoreManager != null)
            _currentRoundScoreManager.OnScoresSubmitted -= HandleScores;
        
        // send round end packets here
        
        if (_points.Any(i => i.Value == 2))
        {
            // end match
            return;
        }

        _currentRoundVoteManager = new VoteManager(_teams.Keys.ToArray(), mapData);
    }

    public void StartMatch() => StartMatchAsync();

    private void HandleClientDisconnect(IConnectedClient client)
    {
        client.OnDisconnected -= HandleClientDisconnect;

        _teams.Remove(client);
        
        _currentRoundScoreManager?.HandlePlayerDisconneced(client);
        _currentRoundVoteManager?.HandlePlayerDisconneced(client);
    }

    private async Task SendPacketToClients(ServerPacket packet, Team? teamFilter = null)
    {
        var players = _teams.Keys.ToList();

        if (teamFilter != null)
            players = players.Where(i => _teams[i] == teamFilter).ToList();

        foreach (var player in players)
            await player.SendPacket(packet);
    }

    public enum Team
    {
        Red,
        Blue
    }
}