#define DEBUG
using LoungeSaber_Server.Models.Client;
using Timer = System.Timers.Timer;

namespace LoungeSaber_Server.Gameplay.Matchmaking;

public static class Matchmaker
{
    private static List<MatchmakingClient> _clientPool = [];

    private static Timer _mmrThresholdTimer = new Timer
    {
        Enabled = true,
        AutoReset = true,
        Interval = 5000
    };

    public static async Task AddClientToPool(ConnectedClient client)
    {
        await Task.Delay(100);
        #if !DEBUG
        _clientPool.Add(new MatchmakingClient(client));
        #else
        var match = new Match.Match(client, new DummyConnectedClient());
        await match.StartMatch();
        #endif
    }
}