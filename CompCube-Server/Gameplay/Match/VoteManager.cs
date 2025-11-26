using CompCube_Models.Models.ClientData;
using CompCube_Models.Models.Map;
using CompCube_Models.Models.Packets.UserPackets;
using CompCube_Server.Interfaces;
using CompCube_Server.SQL;

namespace CompCube_Server.Gameplay.Match;

public class VoteManager
{
    private readonly Random _random = new();
    private readonly MapData _mapData;
    
    public readonly VotingMap[] VotingOptions;

    private readonly Dictionary<UserInfo, VotingMap?> _votes;
    public event Action<VotingMap>? OnMapDetermined;

    public event Action<IConnectedClient, int>? OnMapVotedFor;
    

    private readonly Timer _timer;

    private bool MapAlreadyDetermined { get; set; } = false;

    public VoteManager(Team red, Team blue, MapData mapData)
    {
        _mapData = mapData;

        VotingOptions = GetRandomMapSelections(3);
        
        _timer = new Timer(OnTimerElapsed, null, new TimeSpan(0, 0, 30), Timeout.InfiniteTimeSpan);
        
        red.DoForEach(i =>
        {
            i.OnUserVoted += HandleUserVote;
            i.OnDisconnected += HandleClientDisconnected;
        });
        blue.DoForEach(i =>
        {
            i.OnUserVoted += HandleUserVote;
            i.OnDisconnected += HandleClientDisconnected;
        });

        _votes = red.Players.Concat(blue.Players).Select(i => new KeyValuePair<UserInfo, VotingMap?>(i.UserInfo, null)).ToDictionary();
    }

    private void HandleClientDisconnected(IConnectedClient client)
    {
        client.OnDisconnected -= HandleClientDisconnected;
        client.OnUserVoted -= HandleUserVote;

        _votes.Remove(client.UserInfo);
        
        DetermineVoteIfAllowed();
    }

    private void HandleUserVote(VotePacket vote, IConnectedClient client)
    {
        client.OnUserVoted -= HandleUserVote;

        var map = VotingOptions[vote.VoteIndex];

        _votes[client.UserInfo] = map;
        
        OnMapVotedFor?.Invoke(client, vote.VoteIndex);
        
        DetermineVoteIfAllowed();
    }

    private void OnTimerElapsed(object? _)
    {
        if (MapAlreadyDetermined) 
            return;

        if (_votes.All(i => i.Value == null))
        {
            ChooseVote(VotingOptions[0]);
            return;
        }

        var validVotes = _votes.Values.Where(i => i != null).ToArray();
        
        ChooseVote(validVotes.ElementAt(_random.Next(0, validVotes.Length)) ?? VotingOptions[0]);
    }

    private void DetermineVoteIfAllowed()
    {
        if (_votes.Any(i => i.Value == null))
            return;

        var selectedVote = _random.Next(0, _votes.Count);
        
        ChooseVote(_votes.Values.ToList()[selectedVote] ?? VotingOptions[0]);
    }

    private void ChooseVote(VotingMap map)
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

        if (allMaps.Count == 3)
            return allMaps.ToArray();
        

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