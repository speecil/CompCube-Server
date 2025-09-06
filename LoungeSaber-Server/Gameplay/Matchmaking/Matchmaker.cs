using System.Timers;
using LoungeSaber_Server.Interfaces;
using LoungeSaber_Server.Logging;
using LoungeSaber_Server.Models.Client;
using LoungeSaber_Server.Models.Match;
using LoungeSaber_Server.Models.Packets.ServerPackets;
using LoungeSaber_Server.SQL;
using Timer = System.Timers.Timer;

namespace LoungeSaber_Server.Gameplay.Matchmaking;

public class Matchmaker : IMatchmaker
{
    private readonly UserData _userData;
    private readonly MapData _mapData;
    private readonly MatchLog _matchLog;
    private readonly Logger _logger;
    
    private readonly List<MatchmakingClient> _clientPool = [];
    
    public readonly List<Match.Match> ActiveMatches = [];

    public event Action<Match.Match>? OnMatchStarted;

    public Matchmaker(UserData userData, MapData mapData, MatchLog matchLog, ConnectionManager connectionmanager, Logger logger)
    {
        _userData = userData;
        _mapData = mapData;
        _matchLog = matchLog;
        _logger = logger;
        
        connectionmanager.OnClientJoined += AddClientToPool;
    }

    private void OnMatchEnded(MatchResultsData results, Match.Match match)
    {
        match.OnMatchEnded -= OnMatchEnded;

        ActiveMatches.Remove(match);
    }

    public async void AddClientToPool(ConnectedClient client)
    {
        try
        {
            _clientPool.Add(new MatchmakingClient(client));

            if (_clientPool.Count != 2) 
                return;
            
            var match = new Match.Match(_clientPool[0].Client, _clientPool[1].Client, _matchLog, _userData, _mapData, _logger);
            _clientPool.Clear();
            await match.StartMatch();
            OnMatchStarted?.Invoke(match);
        }
        catch (Exception e)
        {
            _logger.Error(e);
        }
    }
}