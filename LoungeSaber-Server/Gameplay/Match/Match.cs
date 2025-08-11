using System.Diagnostics;
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
    
    public readonly ConnectedClient PlayerOne;
    public readonly ConnectedClient PlayerTwo;

    private const int KFactor = 75;

    private readonly List<(VotingMap, ConnectedClient)> _userVotes = [];

    private ScoreSubmissionPacket? _playerOneScore = null;
    private ScoreSubmissionPacket? _playerTwoScore = null;

    private readonly VotingMap[] _mapSelections;

    private VotingMap? _selectedMap;
    
    public event Action<MatchResultsData, Match>? OnMatchEnded;
    public event Action<ConnectedClient, int, string>? OnPlayerPunished;

    private const int MmrLossOnDisconnect = 50;

    private readonly int _id;

    public Match(ConnectedClient playerOne, ConnectedClient playerTwo, MatchLog matchLog, UserData userData, MapData mapData)
    {
        _matchLog = matchLog;
        _userData = userData;
        _mapData = mapData;
        PlayerOne = playerOne;
        PlayerTwo = playerTwo;
        
        _id = matchLog.GetValidMatchId();
        _mapSelections = GetRandomMapSelections(3);
    }

    public async Task StartMatch()
    {
        PlayerOne.OnDisconnected += OnPlayerDisconnected;
        PlayerTwo.OnDisconnected += OnPlayerDisconnected;
        
        PlayerOne.OnUserVoted += OnUserVoted;
        PlayerTwo.OnUserVoted += OnUserVoted;
        
        await PlayerOne.SendPacket(new MatchCreatedPacket(_mapSelections, PlayerTwo.UserInfo));
        await PlayerTwo.SendPacket(new MatchCreatedPacket(_mapSelections, PlayerOne.UserInfo));
    }

    private async void OnPlayerDisconnected(ConnectedClient client)
    {
        try
        {
            var winner = GetOppositeClient(client).UserInfo;
            var loser = GetOppositeClient(client).UserInfo;
            
            var mmrChange = GetMmrChange(winner, loser);
            
            _userData.ApplyMmrChange(client.UserInfo, -mmrChange - MmrLossOnDisconnect);
            _userData.ApplyMmrChange(GetOppositeClient(client).UserInfo, mmrChange);
            
            OnPlayerPunished?.Invoke(client, 50, "Leaving Match Early");
            
            await GetOppositeClient(client).SendPacket(new PrematureMatchEndPacket("OpponentDisconnected"));
            
            EndMatch(new MatchResultsData(new MatchScore(winner, Score.Empty), new MatchScore(loser, Score.Empty), mmrChange, null, true, _id, DateTime.UtcNow));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    
    

    private void EndMatch(MatchResultsData results)
    {
        PlayerOne.OnDisconnected -= OnPlayerDisconnected;
        PlayerTwo.OnDisconnected -= OnPlayerDisconnected;
        
        PlayerOne.StopListeningToClient();
        PlayerTwo.StopListeningToClient();
        
        _matchLog.AddMatchToTable(results);
        OnMatchEnded?.Invoke(results, this);
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
            Console.WriteLine(e);
        }
    }

    private async void OnScoreSubmitted(ScoreSubmissionPacket score, ConnectedClient client)
    {
        try
        {
            client.OnScoreSubmission -= OnScoreSubmitted;
            client.OnDisconnected -= OnPlayerDisconnected;
        
            if (client.UserInfo.UserId == PlayerOne.UserInfo.UserId) _playerOneScore = score;
            else _playerTwoScore = score;

            if (_playerOneScore == null || _playerTwoScore == null) 
                return;
            
            var winnerScoreAndClient = _playerOneScore.Score > _playerTwoScore.Score ? (_playerOneScore, PlayerOne) : (_playerTwoScore, PlayerTwo);

            if (_playerOneScore.Score == _playerTwoScore.Score)
            {
                winnerScoreAndClient = PlayerOne.UserInfo.Mmr > PlayerTwo.UserInfo.Mmr ? (_playerTwoScore, PlayerTwo) : (_playerOneScore, PlayerOne);
            }
            
            var loserScoreAndClient = winnerScoreAndClient.Item1 == _playerOneScore ? (_playerTwoScore, PlayerTwo) : (_playerOneScore, PlayerOne);
            
            var winnerMatchScore = GetMatchScoreFromScoreSubmission(winnerScoreAndClient.Item1, winnerScoreAndClient.Item2.UserInfo);
            var loserMatchScore =
                GetMatchScoreFromScoreSubmission(loserScoreAndClient.Item1, loserScoreAndClient.Item2.UserInfo);

            var mmrChange = GetMmrChange(winnerScoreAndClient.Item2.UserInfo, loserScoreAndClient.Item2.UserInfo);

            var newWinnerUserData = _userData.ApplyMmrChange(winnerScoreAndClient.Item2.UserInfo, mmrChange);
            var newLoserUserData = _userData.ApplyMmrChange(loserScoreAndClient.Item2.UserInfo, -mmrChange);
            
            await winnerScoreAndClient.Item2.SendPacket(new MatchResultsPacket(loserScoreAndClient.Item1, winnerScoreAndClient.Item1,
                MatchResultsPacket.MatchWinner.You, mmrChange, newLoserUserData, newWinnerUserData));
            await loserScoreAndClient.Item2.SendPacket(new MatchResultsPacket(winnerScoreAndClient.Item1, loserScoreAndClient.Item1, MatchResultsPacket.MatchWinner.Opponent, mmrChange, newWinnerUserData, newLoserUserData));
            
            EndMatch(new MatchResultsData(winnerMatchScore, loserMatchScore, mmrChange, _selectedMap, false, _id, DateTime.UtcNow));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private MatchScore GetMatchScoreFromScoreSubmission(ScoreSubmissionPacket scoreSubmission, UserInfo userInfo)
    {
        return new MatchScore(userInfo, new Score(scoreSubmission.Score, (float) scoreSubmission.Score / scoreSubmission.MaxScore, scoreSubmission.ProMode, scoreSubmission.MissCount, scoreSubmission.FullCombo));
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