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

public class Match
{
    private readonly MatchLog _matchLog;
    private readonly UserData _userData;
    private readonly Logger _logger;
    
    public readonly IConnectedClient PlayerOne;
    public readonly IConnectedClient PlayerTwo;

    private readonly ScoreManager _scoreManager;
    private readonly VoteManager _voteManager;

    private VotingMap? _selectedMap;
    
    public event Action<MatchResultsData, Match>? OnMatchEnded;
    public event Action<IConnectedClient, int, string>? OnPlayerPunished;

    private const int MmrLossOnDisconnect = 50;

    private readonly int _id;
    
    private const int KFactor = 75;

    public Match(IConnectedClient playerOne, IConnectedClient playerTwo, MatchLog matchLog, UserData userData, MapData mapData, Logger logger)
    {
        _matchLog = matchLog;
        _userData = userData;
        _logger = logger;
        
        PlayerOne = playerOne;
        PlayerTwo = playerTwo;

        _scoreManager = new ScoreManager(playerOne, playerTwo);
        _voteManager = new VoteManager(playerOne, playerTwo, mapData);
        
        _id = matchLog.GetValidMatchId();
    }

    public async Task StartMatch()
    {
        _logger.Info($"Match started between {PlayerOne.UserInfo.Username} and {PlayerTwo.UserInfo.Username} ({_id})");
        
        PlayerOne.OnDisconnected += OnPlayerDisconnected;
        PlayerTwo.OnDisconnected += OnPlayerDisconnected;

        _voteManager.OnClientVoted += OnUserVoted;
        _voteManager.OnMapDetermined += OnMapDetermined;
        
        _scoreManager.OnWinnerDetermined += OnWinnerDetermined;
        
        await PlayerOne.SendPacket(new MatchCreatedPacket(_voteManager.VotingOptions, PlayerTwo.UserInfo));
        await PlayerTwo.SendPacket(new MatchCreatedPacket(_voteManager.VotingOptions, PlayerOne.UserInfo));
    }

    private async void OnMapDetermined(VotingMap map)
    {
        try
        {
            _selectedMap = map;
        
            PlayerOne.OnScoreSubmission += OnScoreSubmitted;
            PlayerTwo.OnScoreSubmission += OnScoreSubmitted;

            await Task.Delay(3000);

            await PlayerOne.SendPacket(new MatchStartedPacket(_selectedMap, 15, 10, PlayerTwo.UserInfo));
            await PlayerTwo.SendPacket(new MatchStartedPacket(_selectedMap, 15, 10, PlayerOne.UserInfo));
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
            var winnerClient = winner.User.UserId == PlayerOne.UserInfo.UserId ? PlayerOne : PlayerTwo;
            var loserClient = loser.User.UserId == PlayerOne.UserInfo.UserId ? PlayerOne : PlayerTwo;

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
        PlayerOne.OnDisconnected -= OnPlayerDisconnected;
        PlayerTwo.OnDisconnected -= OnPlayerDisconnected;
        
        _logger.Info($"Match between {PlayerOne.UserInfo.Username} and {PlayerTwo.UserInfo.Username} concluded ({_id})");
        
        PlayerOne.Disconnect();
        PlayerTwo.Disconnect();
        
        _matchLog.AddMatchToTable(results);
        OnMatchEnded?.Invoke(results, this);

        _userData.ApplyMmrChange(results.Winner.User, results.MmrChange);
        
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
            await PlayerOne.SendPacket(packet);
        });
        
        Task.Run(async () =>
        {
            await PlayerTwo.SendPacket(packet);
        });
    }

    private IConnectedClient GetOppositeClient(IConnectedClient client) => client.UserInfo.UserId == PlayerOne.UserInfo.UserId ? PlayerTwo : PlayerOne;
}