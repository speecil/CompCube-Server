using CompCube_Models.Models.Match;
using CompCube_Server.Discord.Events;
using CompCube_Server.Gameplay.Match;
using CompCube_Server.Interfaces;
using CompCube_Server.Logging;
using CompCube_Server.Networking.Client;
using CompCube_Server.SQL;
using CompCube_Server.Models.Client;

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
    
    public event Action<MatchResultsData, Match.Match>? QueueMatchEnded;

    public async void AddClientToPool(IConnectedClient client)
    {
        try
        {
            await Task.Delay(100);
        
            var match = new Match.Match(_matchLog, _userData, _mapData, _logger, _matchMessageManager); 
            await match.StartMatch(new MatchSettings(true), client, new DummyConnectedClient(_userData.GetUserById("0") ?? throw new Exception("Could not find debug user data!")));
            match.OnMatchEnded += MatchOnMatchEnded;
        }
        catch (Exception e)
        {
            _logger.Error(e);
        }
    }

    private void MatchOnMatchEnded(MatchResultsData data, Match.Match match)
    {
        match.OnMatchEnded -= MatchOnMatchEnded;
        
        QueueMatchEnded?.Invoke(data, match);
    }
}