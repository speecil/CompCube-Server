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

public class Match
{
    private readonly MatchLog _matchLog;
    private readonly UserData _userData;
    private readonly MapData _mapData;
    private readonly Logger _logger;
    private readonly MatchMessageManager _matchMessageManager;
    
    private IConnectedClient _playerOne;
    private IConnectedClient _playerTwo;

    private ScoreManager _scoreManager;
    private VoteManager _voteManager;
    
    private MatchSettings _matchSettings;

    private VotingMap? _selectedMap;
    
    public event Action<MatchResultsData, Match>? OnMatchEnded;
    public event Action<IConnectedClient, int, string>? OnPlayerPunished;

    private const int MmrLossOnDisconnect = 50;

    private int _id;
    
    private const int KFactor = 75;

    public Match(MatchLog matchLog, UserData userData, MapData mapData, Logger logger, MatchMessageManager matchMessageManager)
    {
        _matchLog = matchLog;
        _userData = userData;
        _logger = logger;
        _mapData = mapData;
        _matchMessageManager = matchMessageManager;
    }

    public async Task StartMatch(MatchSettings settings, IConnectedClient playerOne, IConnectedClient playerTwo)
    {
        _matchSettings = settings;
        
        _playerOne = playerOne;
        _playerTwo = playerTwo;
        
        _scoreManager = new ScoreManager(playerOne, playerTwo);
        _voteManager = new VoteManager(playerOne, playerTwo, _mapData);
        
        _id = _matchLog.GetValidMatchId();
        
        _logger.Info($"Match started between {_playerOne.UserInfo.Username} and {_playerTwo.UserInfo.Username} ({_id})");
        
        _playerOne.OnDisconnected += OnPlayerDisconnected;
        _playerTwo.OnDisconnected += OnPlayerDisconnected;

        _voteManager.OnClientVoted += OnUserVoted;
        _voteManager.OnMapDetermined += OnMapDetermined;
        
        _scoreManager.OnWinnerDetermined += OnWinnerDetermined;
        
        await _playerOne.SendPacket(new MatchCreatedPacket(_voteManager.VotingOptions, _playerTwo.UserInfo));
        await _playerTwo.SendPacket(new MatchCreatedPacket(_voteManager.VotingOptions, _playerOne.UserInfo));
    }

    private async void OnMapDetermined(VotingMap map)
    {
        try
        {
            _selectedMap = map;
        
            _playerOne.OnScoreSubmission += OnScoreSubmitted;
            _playerTwo.OnScoreSubmission += OnScoreSubmitted;

            await Task.Delay(3000);

            await _playerOne.SendPacket(new MatchStartedPacket(_selectedMap, 15, 10, _playerTwo.UserInfo));
            await _playerTwo.SendPacket(new MatchStartedPacket(_selectedMap, 15, 10, _playerOne.UserInfo));
        }
        catch (Exception e)
        {
            _logger.Error(e);
        }
    }

    private async void OnWinnerDetermined(MatchScore winner, MatchScore loser)
    {
        try
        {
            var winnerClient = winner.User.UserId == _playerOne.UserInfo.UserId ? _playerOne : _playerTwo;
            var loserClient = loser.User.UserId == _playerOne.UserInfo.UserId ? _playerOne : _playerTwo;

            var matchResultsData = new MatchResultsData(winner, loser, GetMmrChange(winner.User, loser.User),
                _selectedMap, false, _id, DateTime.Now);

            var matchResultsPacket = new MatchResultsPacket(matchResultsData);
            
            await winnerClient.SendPacket(matchResultsPacket);
            await loserClient.SendPacket(matchResultsPacket);
            
            EndMatch(matchResultsData);
        }
        catch (Exception e)
        {
            _logger.Error(e);
        }
    }

    private async void OnPlayerDisconnected(IConnectedClient client)
    {
        try
        {
            var winner = GetOppositeClient(client).UserInfo;
            var loser = GetOppositeClient(client).UserInfo;
            
            var mmrChange = GetMmrChange(winner, loser);
            
            OnPlayerPunished?.Invoke(client, 50, "Leaving Match Early");
            
            await GetOppositeClient(client).SendPacket(new PrematureMatchEndPacket("OpponentDisconnected"));
            
            EndMatch(new MatchResultsData(new MatchScore(winner, Score.Empty), new MatchScore(loser, Score.Empty), mmrChange, null, true, _id, DateTime.UtcNow));
        }
        catch (Exception e)
        {
            _logger.Error(e);
        }
    }

    private void EndMatch(MatchResultsData results)
    {
        _playerOne.OnDisconnected -= OnPlayerDisconnected;
        _playerTwo.OnDisconnected -= OnPlayerDisconnected;
        
        _logger.Info($"Match between {_playerOne.UserInfo.Username} and {_playerTwo.UserInfo.Username} concluded ({_id})");
        
        _playerOne.Disconnect();
        _playerTwo.Disconnect();
        
        OnMatchEnded?.Invoke(results, this);

        _userData.ApplyMmrChange(results.Winner.User, results.MmrChange);
        
        if (_matchSettings.LogMatch)
        {
            _matchLog.AddMatchToTable(results);
            _matchMessageManager.PostMatchResults(results);
        }
        
        if (results.Premature)
        {
            _userData.ApplyMmrChange(results.Loser.User, -results.MmrChange - MmrLossOnDisconnect);
            return;
        }
        
        _userData.ApplyMmrChange(results.Loser.User, -results.MmrChange);
    }

    private async void OnUserVoted(IConnectedClient client, int voteIdx)
    {
        try
        {
            await GetOppositeClient(client).SendPacket(new OpponentVotedPacket(voteIdx));
        }
        catch (Exception e)
        {
            _logger.Error(e);
        }
    }

    private void OnScoreSubmitted(ScoreSubmissionPacket score, IConnectedClient client)
    {
        client.OnScoreSubmission -= OnScoreSubmitted;
        client.OnDisconnected -= OnPlayerDisconnected;
    }

    private int GetMmrChange(UserInfo winner, UserInfo loser)
    {
        var p = (1.0 / (1.0 + Math.Pow(10, ((winner.Mmr - loser.Mmr) / 400.0))));

        return (int) (KFactor * p);
    }

    private void SendToBothClients(ServerPacket packet)
    {
        Task.Run(async () =>
        {
            await _playerOne.SendPacket(packet);
        });
        
        Task.Run(async () =>
        {
            await _playerTwo.SendPacket(packet);
        });
    }

    private IConnectedClient GetOppositeClient(IConnectedClient client) => client.UserInfo.UserId == _playerOne.UserInfo.UserId ? _playerTwo : _playerOne;
}