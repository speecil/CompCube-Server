using LoungeSaber_Server.Logging;
using LoungeSaber_Server.Models.Client;
using LoungeSaber_Server.Models.ClientData;
using LoungeSaber_Server.Models.Map;
using LoungeSaber_Server.Models.Match;
using LoungeSaber_Server.Models.Packets;
using LoungeSaber_Server.Models.Packets.ServerPackets;
using LoungeSaber_Server.Models.Packets.UserPackets;
using LoungeSaber_Server.SQL;

namespace LoungeSaber_Server.Gameplay.Match;

public class Match
{
    private readonly MatchLog _matchLog;
    private readonly UserData _userData;
    private readonly MapData _mapData;
    private readonly Logger _logger;
    
    public readonly ConnectedClient PlayerOne;
    public readonly ConnectedClient PlayerTwo;

    private readonly List<(VotingMap, ConnectedClient)> _userVotes = [];

    private readonly ScoreManager _scoreManager;

    private readonly VotingMap[] _mapSelections;

    private VotingMap? _selectedMap;
    
    public event Action<MatchResultsData, Match>? OnMatchEnded;
    public event Action<ConnectedClient, int, string>? OnPlayerPunished;

    private const int MmrLossOnDisconnect = 50;

    private readonly int _id;
    
    private const int KFactor = 75;

    public Match(ConnectedClient playerOne, ConnectedClient playerTwo, MatchLog matchLog, UserData userData, MapData mapData, Logger logger)
    {
        _matchLog = matchLog;
        _userData = userData;
        _mapData = mapData;
        _logger = logger;
        
        PlayerOne = playerOne;
        PlayerTwo = playerTwo;

        _scoreManager = new ScoreManager(playerOne, playerTwo);
        
        _id = matchLog.GetValidMatchId();
        _mapSelections = GetRandomMapSelections(3);
    }

    public async Task StartMatch()
    {
        _logger.Info($"Match started between {PlayerOne.UserInfo.Username} and {PlayerTwo.UserInfo.Username} ({_id})");
        
        PlayerOne.OnDisconnected += OnPlayerDisconnected;
        PlayerTwo.OnDisconnected += OnPlayerDisconnected;
        
        PlayerOne.OnUserVoted += OnUserVoted;
        PlayerTwo.OnUserVoted += OnUserVoted;
        
        _scoreManager.OnWinnerDetermined += OnWinnerDetermined;
        
        await PlayerOne.SendPacket(new MatchCreatedPacket(_mapSelections, PlayerTwo.UserInfo));
        await PlayerTwo.SendPacket(new MatchCreatedPacket(_mapSelections, PlayerOne.UserInfo));
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

    private async void OnPlayerDisconnected(ConnectedClient client)
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

    private async void OnUserVoted(VotePacket vote, ConnectedClient client)
    {
        try
        {
            client.OnUserVoted -= OnUserVoted;
        
            _userVotes.Add((_mapSelections[vote.VoteIndex], client));

            await GetOppositeClient(client).SendPacket(new OpponentVotedPacket(vote.VoteIndex));

            if (_userVotes.Count != 2) 
                return;
            
            var random = new Random();
            
            _selectedMap = _userVotes[random.Next(_userVotes.Count)].Item1;
            
            PlayerOne.OnScoreSubmission += OnScoreSubmitted;
            PlayerTwo.OnScoreSubmission += OnScoreSubmitted;

            await Task.Delay(3000);

            SendToBothClients(new MatchStartedPacket(_selectedMap, DateTime.UtcNow.AddSeconds(15),
                DateTime.UtcNow.AddSeconds(25)));
        }
        catch (Exception e)
        {
            _logger.Error(e);
        }
    }

    private void OnScoreSubmitted(ScoreSubmissionPacket score, ConnectedClient client)
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

    private VotingMap[] GetRandomMapSelections(int amount)
    {
        var random = new Random();
        
        var selections = new List<VotingMap>();
        
        var allMaps = _mapData.GetAllMaps();

        while (selections.Count < amount)
        {
            var randomMap = allMaps[random.Next(0, allMaps.Count)];
            
            if (selections.Any(i => i.Category == randomMap.Category)) 
                continue;
            
            if (selections.Any(i => i.Hash == randomMap.Hash)) 
                continue;
            
            selections.Add(randomMap);
        }

        return selections.ToArray();
    }

    private ConnectedClient GetOppositeClient(ConnectedClient client) => client.UserInfo.UserId == PlayerOne.UserInfo.UserId ? PlayerTwo : PlayerOne;
}