using CompCube_Models.Models.Match;
using CompCube_Server.Discord.Events;
using CompCube_Server.Gameplay.Match;
using CompCube_Server.Interfaces;
using CompCube_Server.Logging;
using CompCube_Server.Models.Client;
using CompCube_Server.SQL;

namespace CompCube_Server.Gameplay.Matchmaking;

public class StandardQueue : IQueue
{
    private readonly UserData _userData;
    private readonly MapData _mapData;
    private readonly MatchLog _matchLog;
    private readonly Logger _logger;
    private readonly MatchMessageManager _matchMessageManager;
    
    private readonly List<MatchmakingClient> _clientPool = [];
    
    public readonly List<Match.Match> ActiveMatches = [];

    public string QueueName => "standard";
    
    public event Action<MatchResultsData, Match.Match>? QueueMatchEnded;

    public StandardQueue(UserData userData, MapData mapData, MatchLog matchLog, Logger logger, MatchMessageManager matchMessageManager)
    {
        _userData = userData;
        _mapData = mapData;
        _matchLog = matchLog;
        _logger = logger;
        _matchMessageManager = matchMessageManager;
    }

    private void OnMatchEnded(MatchResultsData results, Match.Match match)
    {
        match.OnMatchEnded -= OnMatchEnded;
        
        QueueMatchEnded?.Invoke(results, match);

        ActiveMatches.Remove(match);
    }

    public async void AddClientToPool(IConnectedClient client)
    {
        try
        {
            _clientPool.Add(new MatchmakingClient(client));

            if (_clientPool.Count != 2) 
                return;
            
            var match = new Match.Match(_matchLog, _userData, _mapData, _logger, _matchMessageManager);
            await match.StartMatch(new MatchSettings(true), _clientPool[0].Client, _clientPool[1].Client);
            ActiveMatches.Add(match);
            
            _clientPool.Clear();
            
            match.OnMatchEnded += OnMatchEnded;
        }
        catch (Exception e)
        {
            _logger.Error(e);
        }
    }
}