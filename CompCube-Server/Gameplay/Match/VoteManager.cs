using System.Threading;
using CompCube_Models.Models.Map;
using CompCube_Models.Models.Packets.UserPackets;
using CompCube_Server.Interfaces;
using CompCube_Server.SQL;

namespace CompCube_Server.Gameplay.Match;

public class VoteManager : IDisposable
{
    private readonly Random _random = new();
    private readonly MapData _mapData;

    private readonly Dictionary<IConnectedClient, VotingMap?> _playerVotes;

    public readonly VotingMap[] Options;
    private readonly List<VotingMap> _exclude;

    private readonly Action<VotingMap> _voteDecidedCallBack;

    private readonly int _votingTimeSeconds;
    private readonly CancellationTokenSource _cts = new();
    private readonly object _lock = new();
    private bool _decided = false;

    public VoteManager(IConnectedClient[] players, MapData mapData, Action<VotingMap> voteDecidedCallBack, int votingTimeSeconds, List<VotingMap> exclude = null!)
    {
        _mapData = mapData;
        _voteDecidedCallBack = voteDecidedCallBack;
        _votingTimeSeconds = votingTimeSeconds;
        _exclude = exclude ?? [];

        _playerVotes = players.Select(i => new KeyValuePair<IConnectedClient, VotingMap?>(i, null)).ToDictionary();

        Options = GetRandomMapSelection();

        foreach (var player in players)
            player.OnUserVoted += HandlePlayerVote;

        Task.Run(() => WaitAndDecideAsync(_cts.Token));
    }

    private async Task WaitAndDecideAsync(CancellationToken token)
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(_votingTimeSeconds), token);
            DecideVoteIfAllowed(force: true);
        }
        catch (OperationCanceledException)
        {
        }
    }


    private void HandlePlayerVote(VotePacket vote, IConnectedClient client)
    {
        lock (_lock)
        {
            if (_decided)
                return;

            if (!_playerVotes.ContainsKey(client))
                return;

            if (Options == null || Options.Length == 0)
                return;

            if (vote.VoteIndex < 0 || vote.VoteIndex >= Options.Length)
            {
                return;
            }

            _playerVotes[client] = Options[vote.VoteIndex];
        }

        DecideVoteIfAllowed();
    }


    public void HandlePlayerDisconnected(IConnectedClient player)
    {
        lock (_lock)
        {
            if (_decided) return;

            _playerVotes.Remove(player);

            player.OnUserVoted -= HandlePlayerVote;
        }

        DecideVoteIfAllowed();
    }

    private void DecideVoteIfAllowed(bool force = false)
    {
        lock (_lock)
        {
            if (_decided) return;

            if (!force && _playerVotes.Any(i => i.Value == null))
                return;

            VotingMap? map = null;

            var votedMaps = _playerVotes.Values.Where(v => v != null).ToArray();
            if (votedMaps.Length > 0)
            {
                map = votedMaps[_random.Next(0, votedMaps.Length)];
            }
            else
            {
                if (Options.Length > 0)
                    map = Options[_random.Next(0, Options.Length)];
            }

            if (map == null)
                return;

            _decided = true;
            _voteDecidedCallBack?.Invoke(map!);
        }

        try { _cts.Cancel(); } catch { }
    }

    private VotingMap[] GetRandomMapSelection()
    {
        var allMaps = _mapData.GetAllMaps(exclude: _exclude);

        if (allMaps == null || allMaps.Count == 0)
            return Array.Empty<VotingMap>();

        var shuffled = allMaps.OrderBy(_ => _random.Next()).ToList();

        var selected = shuffled
            .DistinctBy(m => m.Hash)
            .Take(3)
            .ToArray();

        return selected;
    }

    public void Dispose()
    {
        try { _cts.Cancel(); } catch { }
        _playerVotes.Keys.ToList().ForEach(i => i.OnUserVoted -= HandlePlayerVote);
        _cts.Dispose();
    }
}