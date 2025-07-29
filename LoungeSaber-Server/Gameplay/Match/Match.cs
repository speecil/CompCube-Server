using System.Diagnostics;
using LoungeSaber_Server.Models.Client;
using LoungeSaber_Server.Models.Map;
using LoungeSaber_Server.Models.Packets;
using LoungeSaber_Server.Models.Packets.ServerPackets;
using LoungeSaber_Server.Models.Packets.UserPackets;
using LoungeSaber_Server.SQL;

namespace LoungeSaber_Server.Gameplay.Match;

public class Match(ConnectedClient playerOne, ConnectedClient playerTwo)
{
    public readonly ConnectedClient PlayerOne = playerOne;
    public readonly ConnectedClient PlayerTwo = playerTwo;

    private readonly List<(VotingMap, ConnectedClient)> _userVotes = [];

    private ScoreSubmissionPacket? _playerOneScore = null;
    private ScoreSubmissionPacket? _playerTwoScore = null;
    
    private readonly VotingMap[] _mapSelections = GetRandomMapSelections(3);
    
    public event Action<MatchResults?, Match>? OnMatchEnded;
    public event Action<ConnectedClient, int, string>? OnPlayerPunished;

    private const int MmrLossOnDisconnect = 50;

    public async Task StartMatch()
    {
        PlayerOne.OnDisconnected += OnPlayerDisconnected;
        PlayerTwo.OnDisconnected += OnPlayerDisconnected;
        
        PlayerOne.OnUserVoted += OnUserVoted;
        PlayerTwo.OnUserVoted += OnUserVoted;
        
        await PlayerOne.SendPacket(new MatchCreated(_mapSelections, PlayerTwo.UserInfo));
        await PlayerTwo.SendPacket(new MatchCreated(_mapSelections, PlayerOne.UserInfo));
    }

    private async void OnPlayerDisconnected(ConnectedClient client)
    {
        try
        {
            var mmrChange = GetMmrChange(GetOppositeClient(client).UserInfo, client.UserInfo);
            
            UserData.Instance.ApplyMmrChange(client.UserInfo, -mmrChange - MmrLossOnDisconnect);
            UserData.Instance.ApplyMmrChange(GetOppositeClient(client).UserInfo, mmrChange);
            
            OnPlayerPunished?.Invoke(client, 50, "Leaving Match Early");
            
            await GetOppositeClient(client).SendPacket(new PrematureMatchEnd("OpponentDisconnected"));
            
            EndMatch(null);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    
    

    private void EndMatch(MatchResults? results)
    {
        PlayerOne.OnDisconnected -= OnPlayerDisconnected;
        PlayerTwo.OnDisconnected -= OnPlayerDisconnected;
        
        PlayerOne.StopListeningToClient();
        PlayerTwo.StopListeningToClient();
        OnMatchEnded?.Invoke(results, this);
    }

    private async void OnUserVoted(VotePacket vote, ConnectedClient client)
    {
        try
        {
            client.OnUserVoted -= OnUserVoted;
        
            _userVotes.Add((_mapSelections[vote.VoteIndex], client));

            await GetOppositeClient(client).SendPacket(new OpponentVoted(vote.VoteIndex));

            if (_userVotes.Count != 2) 
                return;
            
            var random = new Random();
            
            var selectedMap = _userVotes[random.Next(_userVotes.Count)].Item1;
            
            PlayerOne.OnScoreSubmission += OnScoreSubmitted;
            PlayerTwo.OnScoreSubmission += OnScoreSubmitted;

            await Task.Delay(3000);

            SendToBothClients(new MatchStarted(selectedMap, DateTime.UtcNow.AddSeconds(15),
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

            var mmrChange = GetMmrChange(winnerScoreAndClient.Item2.UserInfo, loserScoreAndClient.Item2.UserInfo);

            var newWinnerUserData = UserData.Instance.ApplyMmrChange(winnerScoreAndClient.Item2.UserInfo, mmrChange);
            var newLoserUserData = UserData.Instance.ApplyMmrChange(loserScoreAndClient.Item2.UserInfo, -mmrChange);

            var winnerResults = new MatchResults(loserScoreAndClient.Item1, winnerScoreAndClient.Item1,
                MatchResults.MatchWinner.You, mmrChange, newLoserUserData, newWinnerUserData);
            
            await winnerScoreAndClient.Item2.SendPacket(winnerResults);
            await loserScoreAndClient.Item2.SendPacket(new MatchResults(winnerScoreAndClient.Item1, loserScoreAndClient.Item1, MatchResults.MatchWinner.Opponent, mmrChange, newWinnerUserData, newLoserUserData));
            
            EndMatch(winnerResults);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private int GetMmrChange(UserInfo winner, UserInfo loser)
    {
        // TODO
        return 0;
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

    private static VotingMap[] GetRandomMapSelections(int amount)
    {
        var random = new Random();
        
        var selections = new List<VotingMap>();
        
        var allMaps = MapData.Instance.GetAllMaps();

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