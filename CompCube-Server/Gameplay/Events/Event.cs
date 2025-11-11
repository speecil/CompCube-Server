using CompCube_Models.Models.Events;
using CompCube_Models.Models.Map;
using CompCube_Models.Models.Match;
using CompCube_Models.Models.Packets;
using CompCube_Models.Models.Packets.ServerPackets.Event;
using CompCube_Server.Discord.Events;
using CompCube_Server.Interfaces;

namespace CompCube_Server.Gameplay.Events;

public class Event(EventData eventData, EventMessageManager eventMessageManager) : IQueue
{
    public string QueueName => eventData.EventName;
    
    public EventData EventData => eventData;

    private readonly List<IConnectedClient> _connectedClients = [];
    
    public int ClientCount => _connectedClients.Count;

    private EventController? _eventController;
    
    public void AddClientToPool(IConnectedClient client)
    {
        _connectedClients.Add(client);

        client.OnDisconnected += OnDisconnected;
    }

    private void OnDisconnected(IConnectedClient c)
    {
        c.OnDisconnected -= OnDisconnected;
            
        _connectedClients.Remove(c);
    }
    
    public void StartEvent()
    {
        _eventController = new EventController(_connectedClients, eventMessageManager);
        EventData.AvailableToJoin = false;
        
        _eventController.StartEvent();
    }

    public void SetMap(VotingMap votingMap)
    {
        _eventController.SetActiveMap(votingMap);
    }
}