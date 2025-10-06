using LoungeSaber_Server.Interfaces;
using LoungeSaber_Server.Models.Events;
using LoungeSaber_Server.Models.Match;

namespace LoungeSaber_Server.Gameplay.Events;

public class Event(EventData eventData) : IQueue
{
    public string QueueName => eventData.Name;
    
    public EventData EventData => eventData;
    
    public event Action<MatchResultsData, Match.Match>? QueueMatchEnded;

    private List<IConnectedClient> _connectedClients = [];
    
    public void AddClientToPool(IConnectedClient client)
    {
        _connectedClients.Add(client);
    }
}