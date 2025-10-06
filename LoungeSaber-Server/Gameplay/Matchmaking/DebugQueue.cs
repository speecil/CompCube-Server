using LoungeSaber_Server.Interfaces;
using LoungeSaber_Server.Logging;
using LoungeSaber_Server.Models.Client;
using LoungeSaber_Server.SQL;

namespace LoungeSaber_Server.Gameplay.Matchmaking;

public class DebugQueue : IQueue, IDisposable
{
    private readonly UserData _userData;
    private readonly MatchLog _matchLog;
    private readonly MapData _mapData;
    private readonly Logger _logger;

    private readonly ConnectionManager _connectionManager;
    
    public DebugQueue(UserData userData, MatchLog matchLog, MapData mapData, ConnectionManager connectionManager, Logger logger)
    {
        _mapData = mapData;
        _userData = userData;
        _matchLog = matchLog;
        _connectionManager = connectionManager;
        _logger = logger;

        _connectionManager.OnClientJoined += AddClientToPool;
    }

    public string QueueName => "debug";
    public event Action<Match.Match>? OnMatchStarted;

    public async void AddClientToPool(IConnectedClient client)
    {
        try
        {
            await Task.Delay(100);
        
            var match = new Match.Match(client, new DummyConnectedClient(_userData.GetUserById("0") ?? throw new Exception("Could not find debug user data!")), _matchLog, _userData, _mapData, _logger); 
            OnMatchStarted?.Invoke(match);
            await match.StartMatch();
        }
        catch (Exception e)
        {
            _logger.Error(e);
        }
    }

    public void Dispose()
    {
        _connectionManager.OnClientJoined -= AddClientToPool;
    }
}