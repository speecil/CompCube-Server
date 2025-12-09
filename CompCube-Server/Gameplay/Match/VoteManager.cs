using CompCube_Models.Models.Map;
using CompCube_Models.Models.Packets.UserPackets;
using CompCube_Server.Interfaces;
using CompCube_Server.SQL;

namespace CompCube_Server.Gameplay.Match;

public class VoteManager
{
    private readonly Random _random = new();
    private readonly MapData _mapData;
    
    private readonly Dictionary<IConnectedClient, VotingMap?> _playerVotes;

    public readonly VotingMap[] Options;

    public event Action<IConnectedClient, VotingMap?>? OnPlayerVoted;

    private readonly Action<VotingMap> _voteDecidedCallBack;
    
    public VoteManager(IConnectedClient[] players, MapData mapData, Action<VotingMap> voteDecidedCallBack)
    {
        _mapData = mapData;
        _voteDecidedCallBack = voteDecidedCallBack;
        
        _playerVotes = players.Select(i => new KeyValuePair<IConnectedClient,VotingMap?>(i, null)).ToDictionary();

        Options = GetRandomMapSelection();

        foreach (var player in players)
            player.OnUserVoted += HandlePlayerVote;    
    }

    private void HandlePlayerVote(VotePacket vote, IConnectedClient client)
    {
        _playerVotes[client] = Options[vote.VoteIndex];
        
        OnPlayerVoted?.Invoke(client, Options[vote.VoteIndex]);
    }

    public void HandlePlayerDisconneced(IConnectedClient player)
    {
        _playerVotes.Remove(player);

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
        // debug
        return _mapData.GetAllMaps().ToArray();
        
        var maps = new List<VotingMap>();

        var allMaps = _mapData.GetAllMaps();
        
        while (maps.Count > 3)
        {
            var map = allMaps[_random.Next(0, allMaps.Count)];

            if (maps.Any(i => i.Hash == map.Hash || i.Category == map.Category))
                continue;
            
            maps.Add(map);
        }

        return maps.ToArray();
    }
}