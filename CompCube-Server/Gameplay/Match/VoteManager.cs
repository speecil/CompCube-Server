using CompCube_Models.Models.ClientData;
using CompCube_Models.Models.Map;
using CompCube_Models.Models.Packets.UserPackets;
using CompCube_Server.Interfaces;
using CompCube_Server.SQL;

namespace CompCube_Server.Gameplay.Match;

public class VoteManager : IDisposable
{
    private readonly Random _random = new();
    private readonly MapData _mapData;
    
    private readonly Dictionary<UserInfo, VotingMap?> _playerVotes;

    public readonly VotingMap[] Options;

    private readonly Action<VotingMap> _voteDecidedCallBack;

    private readonly List<IConnectedClient> _clientsTracked;
    
    public VoteManager(IConnectedClient[] players, MapData mapData, Action<VotingMap> voteDecidedCallBack)
    {
        _mapData = mapData;
        _voteDecidedCallBack = voteDecidedCallBack;

        _clientsTracked = players.ToList();
        
        _playerVotes = players.Select(i => new KeyValuePair<UserInfo,VotingMap?>(i.UserInfo, null)).ToDictionary();

        Options = GetRandomMapSelection();

        foreach (var player in players)
            player.OnUserVoted += HandlePlayerVote;    
    }

    private void HandlePlayerVote(VotePacket vote, IConnectedClient client)
    {
        _playerVotes[client.UserInfo] = Options[vote.VoteIndex];
        
        DecideVoteIfAllowed();
    }

    public void HandlePlayerDisconnected(IConnectedClient player)
    {
        _playerVotes.Remove(player.UserInfo);

        player.OnUserVoted -= HandlePlayerVote;
        
        DecideVoteIfAllowed();
    }

    private void DecideVoteIfAllowed()
    {
        if (_playerVotes.Any(i => i.Value == null))
            return;

        var map = _playerVotes.ElementAt(_random.Next(0, _playerVotes.Count)).Value;

        if (map == null)
            DecideVoteIfAllowed();

        _voteDecidedCallBack?.Invoke(map!);
    }

    private VotingMap[] GetRandomMapSelection()
    {
        var maps = new List<VotingMap>();

        var allMaps = _mapData.GetAllMaps();

        if (allMaps.Count < 3)
            return allMaps.ToArray();
        
        while (maps.Count < 3)
        {
            var map = allMaps[_random.Next(0, allMaps.Count)];

            if (maps.Any(i => i.Hash == map.Hash)) // || i.MapCategory == map.MapCategory))
                continue;
            
            maps.Add(map);
        }

        return maps.ToArray();
    }

    public void Dispose() => _clientsTracked.ForEach(i =>
    {
        i.OnUserVoted -= HandlePlayerVote;
        i.OnDisconnected -= HandlePlayerDisconnected;
    });
}