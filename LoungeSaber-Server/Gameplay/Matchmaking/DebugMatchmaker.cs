using LoungeSaber_Server.Interfaces;
using LoungeSaber_Server.Models.Client;
using LoungeSaber_Server.SQL;

namespace LoungeSaber_Server.Gameplay.Matchmaking;

public class DebugMatchmaker : IMatchmaker, IDisposable
{
    private readonly UserData _userData;
    private readonly MatchLog _matchLog;
    private readonly MapData _mapData;

    private readonly ConnectionManager _connectionManager;
    
    public DebugMatchmaker(UserData userData, MatchLog matchLog, MapData mapData, ConnectionManager connectionManager)
    {
        _mapData = mapData;
        _userData = userData;
        _matchLog = matchLog;
        _connectionManager = connectionManager;

        _connectionManager.OnClientJoined += AddClientToPool;
    }
    
    public event Action<Match.Match>? OnMatchStarted;

    public async void AddClientToPool(ConnectedClient client)
    {
        try
        {
            await Task.Delay(100);
        
            var match = new Match.Match(client, new DummyConnectedClient(_userData.GetUserById("0") ?? throw new Exception()), _matchLog, _userData, _mapData); 
            OnMatchStarted?.Invoke(match);
            await match.StartMatch();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public void Dispose()
    {
        _connectionManager.OnClientJoined -= AddClientToPool;
    }
}