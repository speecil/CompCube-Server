using CompCube_Server.Discord.Events;
using System.Collections.Concurrent;
using CompCube_Server.Gameplay.Match;
using CompCube_Server.Interfaces;
using CompCube_Server.Logging;
using CompCube_Server.Models.Client;
using CompCube_Server.SQL;

namespace CompCube_Server.Gameplay.Matchmaking;

public class StandardCompetitiveQueue : StandardQueue
{
    private readonly GameMatchFactory _gameMatchFactory;
    private readonly Logger _logger;
    
    public override string QueueName => "standard_competitive_1v1";
    
    private readonly List<MatchmakingClient> _clientPool = [];

    private readonly ConcurrentQueue<IConnectedClient> _pendingAdds = [];

    private readonly CancellationTokenSource _cts = new();
    private readonly Task _matchmakingTask;

    public StandardCompetitiveQueue(
        Logger logger,
        GameMatchFactory gameMatchFactory)
    {
        _logger = logger;
        _gameMatchFactory = gameMatchFactory;

        _matchmakingTask = Task.Run(MatchmakingLoop, _cts.Token);
    }

    public override void AddClientToPool(IConnectedClient client)
    {
        _pendingAdds.Enqueue(client);
        client.OnDisconnected += OnClientDisconnected;
    }

    private void OnClientDisconnected(IConnectedClient client)
    {
        _clientPool.RemoveAll(c => c.Client == client);
    }

    public void Stop()
    {
        _cts.Cancel();

        try
        {
            _matchmakingTask.Wait();
        }
        catch (AggregateException) { }
    }

    private async Task MatchmakingLoop()
    {
        var token = _cts.Token;
        _logger.Info("Starting matchmaking loop for Standard Competitive Queue.");
        while (!token.IsCancellationRequested)
        {
            try
            {
                _logger.Info($"Matchmaking loop tick. Current pool size: {_clientPool.Count}, pending adds: {_pendingAdds.Count}");
                DrainPendingAdds();

                if (_clientPool.Count >= 2)
                {
                    RunMatchmakingPass();
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Matchmaking loop error: {ex}");
            }

            await Task.Delay(2000, token);
        }
    }

    private void DrainPendingAdds()
    {
        while (_pendingAdds.TryDequeue(out var client))
        {
            _clientPool.Add(new MatchmakingClient(client));
        }
    }

    private void RunMatchmakingPass()
    {
        _logger.Info("Running matchmaking pass.");
        var sorted = _clientPool
            .OrderBy(c => c.Client.UserInfo.Mmr)
            .ToList();

        for (int i = 0; i < sorted.Count - 1;)
        {
            var a = sorted[i];
            var b = sorted[i + 1];

            if (!a.CanMatchWithOtherClient(b))
            {
                i++;
                _logger.Info($"Clients {a.Client.UserInfo.Username} and {b.Client.UserInfo.Username} cannot be matched yet. Skipping.");
                continue;
            }


            _logger.Info($"Matching clients {a.Client.UserInfo.Username} and {b.Client.UserInfo.Username} with MMRs {a.Client.UserInfo.Mmr} and {b.Client.UserInfo.Mmr}.");

            _clientPool.Remove(a);
            _clientPool.Remove(b);
            a.Client.OnDisconnected -= OnClientDisconnected;
            b.Client.OnDisconnected -= OnClientDisconnected;

            var match = _gameMatchFactory.CreateNewMatch(
                [a.Client],
                [b.Client],
                new MatchSettings(true, true, 75, 50)
            );

            match.StartMatch();

            i += 2;
        }
        _logger.Info("Finished matchmaking pass.");
    }
}