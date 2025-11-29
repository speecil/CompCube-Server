using CompCube_Models.Models.ClientData;
using CompCube_Models.Models.Map;
using CompCube_Models.Models.Match;
using CompCube_Models.Models.Packets;
using CompCube_Models.Models.Packets.ServerPackets;
using CompCube_Models.Models.Packets.UserPackets;
using CompCube_Server.Interfaces;
using CompCube_Server.Logging;
using CompCube_Server.SQL;

namespace CompCube_Server.Gameplay.Match;

public class GameMatch(MapData mapData, Logger logger)
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
                player.OnUserVoted += HandlePlayerVoted;
                
                _teams.Add(player, team);
            }
        }
    }

    private async void HandlePlayerVoted(VotePacket vote, IConnectedClient client)
    {
        try
        {
            await SendPacketToClients(new PlayerVotedPacket(vote.VoteIndex, client.UserInfo.UserId), null, [client]);
        }
        catch (Exception e)
        {
            logger.Error(e);
        }
    }

    public async Task StartMatchAsync()
    {
        _currentRoundVoteManager = new VoteManager(_teams.Keys.ToArray(), mapData, HandleVoteDecided);
        // send match start packets here

        await SendPacketToClients(new MatchCreatedPacket(_teams.Where(i => i.Value == Team.Red).Select(i => i.Key.UserInfo).ToArray(), _teams.Where(i => i.Value == Team.Blue).Select(i => i.Key.UserInfo).ToArray()));
        
        StartRound();
    }

    private async void StartRound()
    {
        try
        {
            _currentRoundVoteManager = new VoteManager(_teams.Keys.ToArray(), mapData, HandleVoteDecided);

            await SendPacketToClients(new RoundStartedPacket(_currentRoundVoteManager.Options, 30));
        }
        catch (Exception e)
        {
            logger.Error(e);
        }
    }

    private async void HandleVoteDecided(VotingMap votingMap)
    {
        _currentRoundScoreManager = new ScoreManager(_teams, HandleResults);

        await Task.Delay(3000);

        await SendPacketToClients(new BeginGameTransitionPacket(votingMap, 15, 25));
    }

    private async void HandleResults(Dictionary<IConnectedClient, Score> scores)
    {
        try
        {
            var redPoints = scores.Where(i => _teams[i.Key] == Team.Red).Sum(i => i.Value.Points);
            var bluePoints = scores.Where(i => _teams[i.Key] == Team.Blue).Sum(i => i.Value.Points);

            if (redPoints >= bluePoints)
                _points[Team.Red] += 1;

            if (bluePoints >= redPoints)
                _points[Team.Blue] += 1;

            await SendPacketToClients(new RoundResultsPacket(
                scores.Select(i => new KeyValuePair<UserInfo, Score>(i.Key.UserInfo, i.Value)).ToDictionary(),
                _points[Team.Red], _points[Team.Blue]));
        
            if (_points.Any(i => i.Value == 2))
            {
                // end match
                return;
            }

            StartRound();
        }
        catch (Exception e)
        {
            logger.Error(e);
        }
    }

    public void StartMatch() => StartMatchAsync();

    private void HandleClientDisconnect(IConnectedClient client)
    {
        client.OnDisconnected -= HandleClientDisconnect;

        _teams.Remove(client);
        
        _currentRoundScoreManager?.HandlePlayerDisconneced(client);
        _currentRoundVoteManager?.HandlePlayerDisconneced(client);
    }

    private async Task SendPacketToClients(ServerPacket packet, Team? teamFilter = null, IConnectedClient[]? playerFilter = null)
    {
        var players = _teams.Keys.ToList();

        if (teamFilter != null)
            players = players.Where(i => _teams[i] == teamFilter).ToList();

        if (playerFilter != null)
            players = players.Where(i => !playerFilter.Contains(i)).ToList();

        foreach (var player in players)
            await player.SendPacket(packet);
    }

    public enum Team
    {
        Red,
        Blue
    }
}