using CompCube_Server.Discord.Events;
using CompCube_Server.Gameplay.Match;
using CompCube_Server.Interfaces;
using CompCube_Server.Logging;
using CompCube_Server.Models.Client;
using CompCube_Server.SQL;

namespace CompCube_Server.Gameplay.Matchmaking;

public class StandardCompetitiveQueue : StandardQueue
{
    private readonly MatchLog _matchLog;
    private readonly UserData _userData;
    private readonly MapData _mapData;
    private readonly Logger _logger;
    private readonly MatchMessageManager _matchMessageManager;
    
    public override string QueueName => "standard_competitive_1v1";
    
    private readonly List<MatchmakingClient> _clientPool = [];

    private readonly Thread _matchmakingCheckerThread;

    public StandardCompetitiveQueue(Logger logger, MapData mapData, MatchMessageManager matchMessageManager, UserData userData, MatchLog matchLog)
    {
        _logger = logger;
        _mapData = mapData;
        _matchMessageManager = matchMessageManager;
        _userData = userData;
        _matchLog = matchLog;

        _matchmakingCheckerThread = new Thread(CreateMatchesWhereAvailable);
        _matchmakingCheckerThread.Start();
    }
    
    public override void AddClientToPool(IConnectedClient client)
    {
        _clientPool.Add(new MatchmakingClient(client));
    }

    private void CreateMatchesWhereAvailable()
    {
        var clientsToCheck = _clientPool;
        
        foreach (var client in clientsToCheck)
            foreach (var comparisonClient in clientsToCheck)
                if (client.CanMatchWithOtherClient(comparisonClient))
                {
                    _clientPool.Remove(comparisonClient);
                    _clientPool.Remove(client);

                    var match = new Match.Match(_matchLog, _userData, _mapData, _logger, _matchMessageManager);
                    match.StartMatch(new MatchSettings(true, true), client.Client, comparisonClient.Client);
                }
    }
}