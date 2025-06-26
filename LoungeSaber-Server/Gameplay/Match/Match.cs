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

            await SendToBothClients(new MatchStarted(selectedMap, DateTime.UtcNow.AddSeconds(15),
                DateTime.UtcNow.AddSeconds(25)));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private async Task SendToBothClients(ServerPacket packet)
    {
        await PlayerOne.SendPacket(packet);
        await PlayerTwo.SendPacket(packet);
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