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

public class GameMatch(MapData mapData, Logger logger, UserData userData, MatchLog matchLog, IDiscordBot messageManager, RankingData rankingData) : IDisposable
{
    private MatchSettings _matchSettings;

    private readonly Dictionary<UserInfo, Team> _initialPlayers = new();

    private List<IConnectedClient> _clients = [];
    
    private readonly Dictionary<string, Team> _teams = new();

    private readonly Dictionary<Team, int> _points = new();

    private ScoreManager? _currentRoundScoreManager = null;
    private VoteManager? _currentRoundVoteManager = null;

    private int _roundCount = 0;

    private readonly Dictionary<Team, int> _mmrChanges = new();

    private readonly int _id = matchLog.GetValidMatchId();

    private List<VotingMap> _playedMaps = [];

    // refactor into configuration file at some point
    private const int SendWinningVoteToClientDelayInMilliseconds = 3000;
    private const int VotingTimeInSeconds = 15;
    private const int TransitionToGameTimeInSeconds = 15;
    private const int UnpauseTimeInSeconds = 10;
    
    public void Init(IConnectedClient[] red, IConnectedClient[] blue, MatchSettings settings)
    {
        _matchSettings = settings;

        _clients = red.Concat(blue).ToList();
        
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
                
                _teams.Add(player.UserInfo.UserId, team);
                _initialPlayers.Add(player.UserInfo, team);
            }
        }
    }

    private async void HandlePlayerVoted(VotePacket vote, IConnectedClient client)
    {
        try
        {
            await SendPacketToClientsAsync(new PlayerVotedPacket(vote.VoteIndex, client.UserInfo.UserId), null, [client]);
        }
        catch (Exception e)
        {
            logger.Error(e);
        }
    }

    public async Task StartMatchAsync()
    {
        // await SendPacketToClientsAsync(new MatchCreatedPacket(_teams.Where(i => i.Value == Team.Red).Select(i => i.Key.UserInfo).ToArray(), _teams.Where(i => i.Value == Team.Blue).Select(i => i.Key.UserInfo).ToArray()));

        var redPlayers = _initialPlayers.Where(i => _teams[i.Key.UserId] == Team.Red).Select(i => i.Key).ToArray();
        var bluePlayers = _initialPlayers.Where(i => _teams[i.Key.UserId] == Team.Blue).Select(i => i.Key).ToArray();
        
        await SendPacketToClientsAsync(new MatchCreatedPacket(redPlayers, bluePlayers));
        
        await StartRound();
    }

    private async Task StartRound()
    {
        try
        {
            _roundCount++;
            _currentRoundVoteManager?.Dispose();
            _currentRoundVoteManager = new VoteManager(_clients.ToArray(), mapData, HandleVoteDecided, VotingTimeInSeconds, exclude: _playedMaps);
        
            // await Task.Delay(10);
            await SendPacketToClientsAsync(new RoundStartedPacket(_currentRoundVoteManager.Options, VotingTimeInSeconds, _roundCount));
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
            _playedMaps.Add(votingMap);
            _currentRoundScoreManager?.Dispose();
            _currentRoundScoreManager = new ScoreManager(_clients.ToArray(), HandleResults);

            await Task.Delay(SendWinningVoteToClientDelayInMilliseconds);

            await SendPacketToClientsAsync(new BeginGameTransitionPacket(votingMap, TransitionToGameTimeInSeconds, UnpauseTimeInSeconds));
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
            // stupid bandaid patch that will not work with match sizes of more than 2 people
            var redPoints = scores.First(i => _teams[i.Key.UserInfo.UserId] == Team.Red).Value.Points;
            var bluePoints = scores.First(i => _teams[i.Key.UserInfo.UserId] == Team.Blue).Value.Points;
            
            // if (redPlayers.Any())
            //
            // if (bluePlayers.Any())
            //     bluePoints = bluePlayers.Sum(i => i.Value.Points);
            
            if (redPoints >= bluePoints)
                _points[Team.Red] += 1;

            if (bluePoints >= redPoints)
                _points[Team.Blue] += 1;
        
            if (_points.Any(i => i.Value == 2))
            {
                await EndMatchAsync();
                return;
            }
            
            await SendPacketToClientsAsync(new RoundResultsPacket(
                scores.Select(i => new KeyValuePair<string, Score>(i.Key.UserInfo.UserId, i.Value)).ToDictionary(),
                _points[Team.Red], _points[Team.Blue]));

            await StartRound();
        }
        catch (Exception e)
        {
            logger.Error(e);
        }
    }

    public void StartMatch() => Task.Factory.StartNew(StartMatchAsync, TaskCreationOptions.LongRunning);

    private async Task EndMatchAsync()
    {
        var winningTeam = Team.Red;

        if (_points[Team.Blue] > _points[Team.Red])
            winningTeam = Team.Blue;
        
        var mmrChange = _mmrChanges[winningTeam];
        
        await SendPacketToClientsAsync(new MatchResultsPacket(mmrChange, _points[Team.Red], _points[Team.Blue]));

        var winningPlayers = _initialPlayers.Where(i => i.Value == winningTeam)
            .Select(i => i.Key).ToArray();
        var losingPlayers = _initialPlayers.Where(i => i.Value != winningTeam)
            .Select(i => i.Key).ToArray();
        
        var matchResultsData = new MatchResultsData(winningPlayers, losingPlayers, mmrChange, false, _id, DateTime.Now);

        if (_matchSettings.LogMatch)
        {
            matchLog.AddMatchToTable(matchResultsData);
            messageManager.PostMatchResults(matchResultsData);
        }

        if (_matchSettings.Competitive)
        {
            rankingData.UpdateUserDataFromMatch(matchResultsData, mmrChange, _matchSettings.MmrPenaltyOnDisconnect);
        }
        
        Dispose();
    }

    private async Task EndMatchPrematurely(string reason, Team? forcedWinningTeam = null)
    {
        await SendPacketToClientsAsync(new PrematureMatchEndPacket(reason));

        var winningTeam = _points.Max().Key;
        
        if (forcedWinningTeam != null)
            winningTeam = forcedWinningTeam.Value;
        
        var matchResults = new MatchResultsData(
            _initialPlayers.Where(i => i.Value == winningTeam).Select(i => i.Key).ToArray(), 
            _initialPlayers.Where(i => i.Value != winningTeam).Select(i => i.Key).ToArray(), 
            _mmrChanges[winningTeam], 
            true, 
            _id, 
            DateTime.Now);
        
        if (_matchSettings.LogMatch)
            matchLog.AddMatchToTable(matchResults);
    }

    private async void HandleClientDisconnect(IConnectedClient client)
    {
        try
        {
            client.OnDisconnected -= HandleClientDisconnect;

            if (_teams.All(i => i.Value != _teams[client.UserInfo.UserId]))
            {
                var losingTeam = _teams[client.UserInfo.UserId];
                var winningTeam = losingTeam == Team.Red ? Team.Blue : Team.Red;
                
                await EndMatchPrematurely("OpponentsDisconnected", winningTeam);
                return;
            }

            _clients.Remove(client);
        
            _currentRoundScoreManager?.HandlePlayerDisconneced(client);
            _currentRoundVoteManager?.HandlePlayerDisconnected(client);
        }
        catch (Exception e)
        {
            logger.Error(e);
        }
    }
    
    private async Task SendPacketToClientsAsync(ServerPacket packet, Team? teamFilter = null, IConnectedClient[]? playerFilter = null)
    {
        var players = _clients.ToList();

        if (teamFilter != null)
            players = players.Where(i => _teams[i.UserInfo.UserId] == teamFilter).ToList();

        if (playerFilter != null)
            players = players.Where(i => !playerFilter.Contains(i)).ToList();

        foreach (var player in players)
        {
            await player.SendPacket(packet);
        }
    }

    private void DoForEachClient(Action<IConnectedClient> action)
    {
        var players = _clients.ToList();
        
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
        Blue,
        FreeForAll
    }

    public void Dispose()
    {
        _currentRoundScoreManager?.Dispose();
        _currentRoundVoteManager?.Dispose();
        
        DoForEachClient(i =>
        {
            i.OnUserVoted -= HandlePlayerVoted;
            i.OnDisconnected -= HandleClientDisconnect;
            
            i.Disconnect();
        });
    }
}