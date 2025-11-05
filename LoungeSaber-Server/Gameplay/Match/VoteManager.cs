using System.Configuration;
using LoungeSaber_Server.Interfaces;
using LoungeSaber_Server.Models.Client;
using LoungeSaber_Server.Models.Map;
using LoungeSaber_Server.Models.Packets.UserPackets;
using LoungeSaber_Server.SQL;

namespace LoungeSaber_Server.Gameplay.Match;

public class VoteManager
{
    private readonly Random _random = new();
    
    private readonly MapData _mapData;
    
    public readonly VotingMap[] VotingOptions;

    private VotingMap? _firstVote = null;

    public event Action<VotingMap>? OnMapDetermined;
    
    public event Action<IConnectedClient, int>? OnClientVoted;

    private readonly Timer _timer;

    private bool MapAlreadyDetermined { get; set; } = false;

    public VoteManager(IConnectedClient playerOne, IConnectedClient playerTwo, MapData mapData)
    {
        _mapData = mapData;

        VotingOptions = GetRandomMapSelections(3);
        
        _timer = new Timer(OnTimerElapsed, null, new TimeSpan(0, 0, 30), Timeout.InfiniteTimeSpan);
        
        playerOne.OnUserVoted += OnUserVoted;
        playerTwo.OnUserVoted += OnUserVoted;
    }

    private void OnTimerElapsed(object? _)
    {
        if (MapAlreadyDetermined) 
            return;

        if (_firstVote == null)
        {
            DetermineVote(VotingOptions[0]);
            return;
        }
        
        DetermineVote(_firstVote);
    }

    private void OnUserVoted(VotePacket packet, IConnectedClient client)
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
            DetermineVote(_firstVote);
            return;
        }
        
        DetermineVote(secondVote);
    }

    private void DetermineVote(VotingMap map)
    {
        if (MapAlreadyDetermined)
            return;
        
        OnMapDetermined?.Invoke(map);
        MapAlreadyDetermined = true;
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