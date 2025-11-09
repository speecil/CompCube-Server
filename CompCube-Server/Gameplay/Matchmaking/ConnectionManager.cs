using System.Net;
using System.Net.Sockets;
using System.Text;
using CompCube_Models.Models.Packets;
using CompCube_Models.Models.Packets.ServerPackets;
using CompCube_Models.Models.Packets.UserPackets;
using CompCube_Server.Logging;
using CompCube_Server.Networking.Client;
using CompCube_Server.SQL;

namespace CompCube_Server.Gameplay.Matchmaking;

public class ConnectionManager : IDisposable
{
    private readonly UserData _userData;
    private readonly Logger _logger;
    private readonly QueueManager _queueManager;
    
    private readonly TcpListener _listener = new(IPAddress.Any, 8008);

    private readonly Thread _listenForClientsThread;
    
    private bool _isStarted = false;
    
    public ConnectionManager(UserData userData, Logger logger, QueueManager queueManager)
    {
        _userData = userData;
        _logger = logger;
        _queueManager = queueManager;
        
        _listenForClientsThread = new Thread(ListenForClients);
        Start();
    }

    public void Start()
    {
        _listener.Start();
        _isStarted = true;
        
        _listenForClientsThread.Start();
        _logger.Info("Started listening for clients");
    }

    private async void ListenForClients()
    {
        try
        {
            while (_isStarted)
            {
                var client = await _listener.AcceptTcpClientAsync();
                
                try
                {
                    var buffer = new byte[1024];

                    var streamLength = client.GetStream().Read(buffer, 0, buffer.Length);
                    buffer = buffer[..streamLength];

                    var json = Encoding.UTF8.GetString(buffer);
                    
                    _logger.Info(json);

                    var packet = UserPacket.Deserialize(json) as JoinRequestPacket ?? throw new Exception("Could not deserialize packet!");

                    var connectedClient = new ConnectedClient(client, _userData.UpdateUserDataOnLogin(packet.UserId, packet.UserName), _logger);

                    var targetMatchmaker = _queueManager.GetQueueFromName(packet.Queue);

                    if (packet.Queue == "")
                        targetMatchmaker = _queueManager.GetQueueFromName("standard");
                    
                    if (targetMatchmaker == null)
                    {
                        await connectedClient.SendPacket(new JoinResponsePacket(false, "Invalid Queue"));
                        connectedClient.Disconnect();
                        continue;
                    }
                    
                    await connectedClient.SendPacket(new JoinResponsePacket(true, "success"));
                    
                    targetMatchmaker.AddClientToPool(connectedClient);
                  
                    _logger.Info($"User {connectedClient.UserInfo.Username} ({connectedClient.UserInfo.UserId}) joined queue {targetMatchmaker.QueueName}");
                }
                catch (Exception e)
                {
                    _logger.Error(e);
                    client.Close();
                }
            }
        }
        catch (Exception e)
        {
            _logger.Error(e);
        }
    }

    private void Stop()
    {
        _isStarted = false;
        _listener.Stop();
    }

    public void Dispose()
    {
        Stop();
    }
}