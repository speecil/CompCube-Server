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
    private readonly ConnectedClient PlayerOne = playerOne;
    private readonly ConnectedClient PlayerTwo = playerTwo;

    private readonly List<(VotingMap, ConnectedClient)> _userVotes = [];

    private ScoreSubmissionPacket? _playerOneScore = null;
    private ScoreSubmissionPacket? _playerTwoScore = null;
    
    private readonly VotingMap[] _mapSelections = GetRandomMapSelections(3);

    public async Task StartMatch()
    {
        PlayerOne.OnUserVoted += OnUserVoted;
        PlayerTwo.OnUserVoted += OnUserVoted;
        
        await PlayerOne.SendPacket(new MatchCreated(_mapSelections, PlayerTwo.UserInfo));
        await PlayerTwo.SendPacket(new MatchCreated(_mapSelections, PlayerOne.UserInfo));
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

            var mmrChange = GetMmrChange(winnerScoreAndClient.Item2, loserScoreAndClient.Item2);
            
            await winnerScoreAndClient.Item2.SendPacket(new MatchResults(loserScoreAndClient.Item1, MatchResults.MatchWinner.You, mmrChange));
            await loserScoreAndClient.Item2.SendPacket(new MatchResults(winnerScoreAndClient.Item1, MatchResults.MatchWinner.Opponent, mmrChange));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private int GetMmrChange(ConnectedClient winner, ConnectedClient loser)
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
            
            if (selections.Any(i => i.Category == randomMap.Category)) continue;
            if (selections.Any(i => i.Hash == randomMap.Hash)) continue;
            
            selections.Add(randomMap);
        }

        return selections.ToArray();
    }

    private ConnectedClient GetOppositeClient(ConnectedClient client) => client.UserInfo.UserId == PlayerOne.UserInfo.UserId ? PlayerTwo : PlayerOne;
}