using CompCube_Server.Discord.Events;
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

    private readonly Thread _matchmakingCheckerThread;

    public StandardCompetitiveQueue(Logger logger, GameMatchFactory gameMatchFactory)
    {
        _logger = logger;
        _gameMatchFactory = gameMatchFactory;

        _matchmakingCheckerThread = new Thread(CreateMatchesWhereAvailable);
        _matchmakingCheckerThread.Start();
    }
    
    public override void AddClientToPool(IConnectedClient client)
    {
        _clientPool.Add(new MatchmakingClient(client));
    }

    private void CreateMatchesWhereAvailable()
    {
        // create shallow copy
        var clientsToCheck = _clientPool.ToArray();
        
        foreach (var client in clientsToCheck)
            foreach (var comparisonClient in clientsToCheck)
                if (client.CanMatchWithOtherClient(comparisonClient))
                {
                    _clientPool.Remove(comparisonClient);
                    _clientPool.Remove(client);

                    var match = _gameMatchFactory.CreateNewMatch([client.Client], [comparisonClient.Client], new MatchSettings(true, true));
                    match.StartMatch();
                }
    }
}