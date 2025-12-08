using CompCube_Models.Models.ClientData;
using CompCube_Models.Models.Map;
using CompCube_Models.Models.Match;
using CompCube_Models.Models.Packets;
using CompCube_Models.Models.Packets.ServerPackets;
using CompCube_Models.Models.Packets.UserPackets;
using CompCube_Server.Discord.Events;
using CompCube_Server.Interfaces;
using CompCube_Server.Logging;
using CompCube_Server.SQL;

namespace CompCube_Server.Gameplay.Match;

public class GameMatch(MapData mapData, Logger logger, UserData userData, MatchLog matchLog, MatchMessageManager messageManager)
{
    private MatchSettings _matchSettings;

    private readonly Dictionary<UserInfo, Team> _initialPlayers = new();

    private readonly Dictionary<IConnectedClient, Team> _teams = new();

    private readonly Dictionary<Team, int> _points = new();

    private ScoreManager? _currentRoundScoreManager = null;
    private VoteManager? _currentRoundVoteManager = null;

    private int _roundCount = 0;

    private readonly Dictionary<Team, int> _mmrChanges = new();

    private readonly int _id = matchLog.GetValidMatchId();
    
    public void Init(IConnectedClient[] red, IConnectedClient[] blue, MatchSettings settings)
    {
        _matchSettings = settings;
        
        SetupTeam(red, Team.Red);
        SetupTeam(blue, Team.Blue);
        
        _points.Add(Team.Red, 0);
        _points.Add(Team.Blue, 0);
        
        _mmrChanges[Team.Red] = GetMmrChange(red.Select(i => i.UserInfo).ToArray(), blue.Select(i => i.UserInfo).ToArray());
        _mmrChanges[Team.Blue] = GetMmrChange(blue.Select(i => i.UserInfo).ToArray(), red.Select(i => i.UserInfo).ToArray());
        
        return;
        
        void SetupTeam(IConnectedClient[] players, Team team)
        {
            foreach (var player in players)
            {
                player.OnDisconnected += HandleClientDisconnect;
                player.OnUserVoted += HandlePlayerVoted;
                
                _teams.Add(player, team);
                _initialPlayers.Add(player.UserInfo, team);
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

        await SendPacketToClients(new MatchCreatedPacket(_teams.Where(i => i.Value == Team.Red).Select(i => i.Key.UserInfo).ToArray(), _teams.Where(i => i.Value == Team.Blue).Select(i => i.Key.UserInfo).ToArray()));
        
        StartRound();
    }

    private async void StartRound()
    {
        try
        {
            _roundCount++;
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
        try
        {
            _currentRoundScoreManager = new ScoreManager(_teams, HandleResults);

            await Task.Delay(3000);

            await SendPacketToClients(new BeginGameTransitionPacket(votingMap, 15, 25));
        }
        catch (Exception e)
        {
            logger.Error(e);
        }
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
        
            if (_points.Any(i => i.Value == 2))
            {
                await EndMatchAsync();
                return;
            }
            
            await SendPacketToClients(new RoundResultsPacket(
                scores.Select(i => new KeyValuePair<UserInfo, Score>(i.Key.UserInfo, i.Value)).ToDictionary(),
                _points[Team.Red], _points[Team.Blue]));

            StartRound();
        }
        catch (Exception e)
        {
            logger.Error(e);
        }
    }

    public void StartMatch() => StartMatchAsync();

    private async Task EndMatchAsync()
    {
        var winningTeam = _points.Max().Key;
        
        var mmrChange = _mmrChanges[winningTeam];
        
        DoForEachClient(i => i.OnDisconnected -= HandleClientDisconnect);
        
        await SendPacketToClients(new MatchResultsPacket(mmrChange, _points[Team.Red], _points[Team.Blue]));
        
        DoForEachClient(i => i.Disconnect());

        var winningPlayers = _initialPlayers.Where(i => i.Value == winningTeam)
            .Select(i => i.Key).ToArray();
        var losingPlayers = _initialPlayers.Where(i => i.Value != winningTeam)
            .Select(i => i.Key).ToArray();
        
        var matchResultsData = new MatchResultsData(winningPlayers, losingPlayers, mmrChange, false, _id, DateTime.Now);
        
        matchLog.AddMatchToTable(matchResultsData);
        
        messageManager.PostMatchResults(matchResultsData);
    }

    private async Task EndMatchPrematurely(string reason)
    {
        await SendPacketToClients(new PrematureMatchEndPacket(reason));

        var winningTeam = _points.Max().Key;
        
        var matchResults = new MatchResultsData(_initialPlayers.Where(i => i.Value == winningTeam).Select(i => i.Key).ToArray(), _initialPlayers.Where(i => i.Value != winningTeam).Select(i => i.Key).ToArray(), _mmrChanges[winningTeam], true, _id, DateTime.Now);
        
        matchLog.AddMatchToTable(matchResults);
    }

    private void HandleClientDisconnect(IConnectedClient client)
    {
        client.OnDisconnected -= HandleClientDisconnect;
        
        userData.SetMmr(client.UserInfo, client.UserInfo.Mmr - 50);

        if (_teams.All(i => i.Value != _teams[client]))
        {
            EndMatchPrematurely("OpponentsDisconnected");
            return;
        }

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

    private void DoForEachClient(Action<IConnectedClient> action)
    {
        var players = _teams.Keys.ToList();
        
        foreach (var player in players)
            action.Invoke(player);
    }
    
    private int GetMmrChange(UserInfo[] winner, UserInfo[] loser)
    {
        if (_matchSettings.Competitive)
            return 0;

        var avgWinnerMmr = winner.Sum(i => i.Mmr) / winner.Length;
        var avgLoserMmr = loser.Sum(i => i.Mmr) / winner.Length;
        
        var p = (1.0 / (1.0 + Math.Pow(10, ((avgWinnerMmr - avgLoserMmr) / 400.0))));

        return (int) (_matchSettings.KFactor * p);
    }

    public enum Team
    {
        Red,
        Blue
    }
}