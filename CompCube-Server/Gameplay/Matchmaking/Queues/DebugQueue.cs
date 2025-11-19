using CompCube_Server.Discord.Events;
using CompCube_Server.Gameplay.Match;
using CompCube_Server.Interfaces;
using CompCube_Server.Logging;
using CompCube_Server.Networking.Client;
using CompCube_Server.SQL;

namespace CompCube_Server.Gameplay.Matchmaking;

public class DebugQueue : IQueue
{
    private readonly UserData _userData;
    private readonly MatchLog _matchLog;
    private readonly MapData _mapData;
    private readonly Logger _logger;
    private readonly MatchMessageManager _matchMessageManager;
    
    public DebugQueue(UserData userData, MatchLog matchLog, MapData mapData, Logger logger, MatchMessageManager matchMessageManager)
    {
        _mapData = mapData;
        _userData = userData;
        _matchLog = matchLog;
        _logger = logger;
        _matchMessageManager = matchMessageManager;
    }

    public string QueueName => "debug";

    public void AddClientToPool(IConnectedClient client)
    {
        var match = new Match.Match(_matchLog, _userData, _mapData, _logger, _matchMessageManager);
        match.StartMatch(new MatchSettings(false, false), client, new DummyConnectedClient(_userData.GetUserById("0") ?? throw new Exception("Could not find debug user data!")));
    }
}