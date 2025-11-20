using CompCube_Models.Models.ClientData;
using CompCube_Models.Models.Map;
using CompCube_Models.Models.Match;
using CompCube_Models.Models.Packets;
using CompCube_Models.Models.Packets.ServerPackets.Event;
using CompCube_Server.Discord.Events;
using CompCube_Server.Interfaces;
using Exception = System.Exception;

namespace CompCube_Server.Gameplay.Events;

public class EventController
{
    private readonly EventMessageManager _eventMessageManager;
    
    private readonly List<IConnectedClient> _connectedClients;
    
    private VotingMap? _map;
    
    private EventPointsController _eventPointsController;
    
    public EventController(List<IConnectedClient> clients, EventMessageManager eventMessageManager)
    {
        _eventMessageManager = eventMessageManager;
        
        _connectedClients = clients;
        _eventPointsController = new EventPointsController(_connectedClients);
        
        _eventPointsController.OnScoresUpdated += OnScoresUpdated;
        _eventPointsController.OnPointsUpdated += OnPointsUpdated;
    }

    private void OnPointsUpdated(Dictionary<UserInfo, int> points)
    {
        _eventMessageManager.PostEventPoints(points);
    }

    private void OnScoresUpdated(List<MatchScore> scores, List<UserInfo> usersWithoutScores)
    {
        _eventMessageManager.PostEventScores(scores, usersWithoutScores);
    }

    public void StartEvent()
    {
        SendPacketToAllClients(new EventStartedPacket());
        
        _eventPointsController = new EventPointsController(_connectedClients);
    }

    public void SetActiveMap(VotingMap map)
    {
        _map = map;
        SendPacketToAllClients(new EventMapSelected(map));
    }

    public void StartPlay()
    {
        if (_map == null)
            throw new Exception("No map selected!");
        
        SendPacketToAllClients(new EventMatchStartedPacket(_map));
    }
    
    

    private void SendPacketToAllClients(ServerPacket packet)
    {
        _connectedClients.ForEach(client => client.SendPacket(packet));
    }
}