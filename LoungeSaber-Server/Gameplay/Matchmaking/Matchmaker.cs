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

    private readonly Timer _mmrThresholdTimer = new Timer
    {
        Enabled = true,
        AutoReset = true,
        Interval = 5000
    };

    public Matchmaker(UserData userData, MapData mapData, MatchLog matchLog, ConnectionManager connectionmanager, Logger logger)
    {
        _userData = userData;
        _mapData = mapData;
        _matchLog = matchLog;
        _logger = logger;
        
        _mmrThresholdTimer.Elapsed += MatchmakingTimerElapsed;
    }

    private void MatchmakingTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        if (_clientPool.Count < 2) 
            return;
        
        var playerOne =  _clientPool[0];
        var playerTwo =  _clientPool[1];
        
        var match = new Match.Match(playerOne.Client, playerTwo.Client, _matchLog, _userData, _mapData, _logger);

        _clientPool.Remove(playerOne);
        _clientPool.Remove(playerTwo);
        
        ActiveMatches.Add(match);
        OnMatchStarted?.Invoke(match);
        
        match.OnMatchEnded += OnMatchEnded;
        
        Task.Run(async () =>
        {
            await match.StartMatch();
        });
    }

    private void OnMatchEnded(MatchResultsData results, Match.Match match)
    {
        match.OnMatchEnded -= OnMatchEnded;

        ActiveMatches.Remove(match);
    }

    public void AddClientToPool(ConnectedClient client)
    {
        _clientPool.Add(new MatchmakingClient(client));
    }
}