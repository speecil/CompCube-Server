using LoungeSaber_Server.Models.Client;
using LoungeSaber_Server.Models.Map;
using LoungeSaber_Server.Models.Packets.UserPackets;
using LoungeSaber_Server.SQL;

namespace LoungeSaber_Server.Gameplay.Match;

public class VoteManager
{
    private readonly Random _random = new Random();
    
    private readonly MapData _mapData;
    
    public readonly VotingMap[] VotingOptions;

    private VotingMap? _firstVote = null;

    public event Action<VotingMap>? OnMapDetermined;
    
    public event Action<ConnectedClient, int>? OnClientVoted;

    public VoteManager(ConnectedClient playerOne, ConnectedClient playerTwo, MapData mapData)
    {
        _mapData = mapData;

        VotingOptions = GetRandomMapSelections(3);
        
        playerOne.OnUserVoted += OnUserVoted;
        playerTwo.OnUserVoted += OnUserVoted;
    }

    private void OnUserVoted(VotePacket packet, ConnectedClient client)
    {
        client.OnUserVoted -= OnUserVoted;

        OnClientVoted?.Invoke(client, packet.VoteIndex);
        
        if (_firstVote == null)
        {
            _firstVote = VotingOptions[packet.VoteIndex];
            return;
        }
        
        var secondVote = VotingOptions[packet.VoteIndex];

        var selectedMap = _random.Next(0, 2);

        if (selectedMap == 0)
        {
            OnMapDetermined?.Invoke(_firstVote);
            return;
        }
        OnMapDetermined?.Invoke(secondVote);
    }

    private VotingMap[] GetRandomMapSelections(int amount)
    {
        var selections = new List<VotingMap>();
        
        var allMaps = _mapData.GetAllMaps();

        while (selections.Count < amount)
        {
            var randomMap = allMaps[_random.Next(0, allMaps.Count)];
            
            if (selections.Any(i => i.Category == randomMap.Category)) 
                continue;
            
            if (selections.Any(i => i.Hash == randomMap.Hash)) 
                continue;
            
            selections.Add(randomMap);
        }

        return selections.ToArray();
    }
}