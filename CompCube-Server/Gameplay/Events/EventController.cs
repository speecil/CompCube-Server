using CompCube_Models.Models.ClientData;
using CompCube_Models.Models.Map;
using CompCube_Models.Models.Packets;
using CompCube_Models.Models.Packets.ServerPackets.Event;
using CompCube_Models.Models.Packets.UserPackets;
using CompCube_Server.Interfaces;

namespace CompCube_Server.Gameplay.Events;

public class EventController
{
    private readonly List<IConnectedClient> _connectedClients;
    
    private VotingMap? _map;
    
    private readonly EventPointsController _eventPointsController;
    
    public EventController(List<IConnectedClient> clients)
    {
        _connectedClients = clients;
        _eventPointsController = new EventPointsController(_connectedClients);
    }

    public void StartEvent()
    {
        SendPacketToAllClients(new EventStartedPacket());
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
        
        _connectedClients.ForEach(client =>
        {
            client.OnScoreSubmission += OnClientScoreSubmission;
        });
    }

    private void OnClientScoreSubmission(ScoreSubmissionPacket score, IConnectedClient client)
    {
        client.OnScoreSubmission -= OnClientScoreSubmission;
        
        
    }

    private void SendPacketToAllClients(ServerPacket packet)
    {
        _connectedClients.ForEach(client => client.SendPacket(packet));
    }
}