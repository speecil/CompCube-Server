using CompCube_Models.Models.Match;
using CompCube_Server.Gameplay.Events;
using CompCube_Server.Interfaces;
using CompCube_Server.Logging;

namespace CompCube_Server.Gameplay.Matchmaking;

public class QueueManager
{
    private readonly IQueue[] _staticQueues;
    private readonly EventManager _eventManager;
    
    public event Action<MatchResultsData, Match.Match>? OnAnyMatchEnded;
    
    public QueueManager(IEnumerable<IQueue> staticQueues, Logger logger, EventManager eventManager)
    {
        _eventManager = eventManager;
        _staticQueues = staticQueues.ToArray();
        
        logger.Info($"Initialized with {_staticQueues.Length} queue(s)");
        
        foreach (var queue in _staticQueues)
            queue.QueueMatchEnded += OnQueueMatchEnded;
        
        _eventManager.EventMatchEnded += OnQueueMatchEnded;
    }

    private void OnQueueMatchEnded(MatchResultsData data, Match.Match match)
    {
        OnAnyMatchEnded?.Invoke(data, match);
    }

    public IQueue? GetQueueFromName(string name)
    {
        var activeEventQueue = _eventManager.ActiveEvents.FirstOrDefault(i => i.QueueName == name);
        
        if (activeEventQueue != null)
            return activeEventQueue;
        
        return _staticQueues.FirstOrDefault(i => i.QueueName == name);
    }
}