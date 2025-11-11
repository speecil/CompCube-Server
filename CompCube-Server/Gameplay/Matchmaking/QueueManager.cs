using CompCube_Models.Models.Match;
using CompCube_Server.Discord.Events;
using CompCube_Server.Gameplay.Events;
using CompCube_Server.Interfaces;
using CompCube_Server.Logging;

namespace CompCube_Server.Gameplay.Matchmaking;

public class QueueManager
{
    private readonly IQueue[] _staticQueues;
    private readonly EventsManager _eventsManager;
    
    public QueueManager(IEnumerable<IQueue> staticQueues, Logger logger, EventsManager eventsManager)
    {
        _eventsManager = eventsManager;
        _staticQueues = staticQueues.ToArray();
        
        logger.Info($"Initialized with {_staticQueues.Length} queue(s)");
    }

    public IQueue? GetQueueFromName(string name)
    {
        var activeEventQueue = _eventsManager.ActiveEvents.FirstOrDefault(i => i.QueueName == name);
        
        if (activeEventQueue != null)
            return activeEventQueue;
        
        return _staticQueues.FirstOrDefault(i => i.QueueName == name);
    }
}