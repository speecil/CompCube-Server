using System.Net;
using System.Net.Sockets;
using System.Text;
using CompCube_Models.Models.Packets;
using CompCube_Models.Models.Packets.ServerPackets;
using CompCube_Models.Models.Packets.UserPackets;
using CompCube_Server.Interfaces;
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
    
    private readonly List<IConnectedClient> _connectedClients = [];
    
    public ConnectionManager(UserData userData, Logger logger, QueueManager queueManager)
    {
        _userData = userData;
        _logger = logger;
        _queueManager = queueManager;
        
        Start();
    }

    public void Start()
    {
        _listener.Start();
        Task.Factory.StartNew(ListenForClients, TaskCreationOptions.LongRunning);
        Task.Factory.StartNew(PollAllClients, TaskCreationOptions.LongRunning);
        
        _logger.Info("Started listening for clients");
    }

    private async Task PollAllClients()
    {
        while (true)
        {
            await Task.Delay(5000);
            
            var clientsToPoll = _connectedClients.ToArray();

            foreach (var client in clientsToPoll)
            {
                try
                {
                    if (client.IsConnectionAlive)
                        continue;
                    client.Disconnect();
                }
                catch (Exception e)
                {
                    _logger.Error($"Client {client.UserInfo.UserId} could not be polled for disconnection! {e}");
                }
            }
            
            // _logger.Info("polled!");
        }
    }

    private async Task ListenForClients()
    {
        while (true)
        {
            var client = await _listener.AcceptTcpClientAsync();

            try
            {
                var buffer = new byte[1024];

                var streamLength = client.GetStream().Read(buffer, 0, buffer.Length);
                buffer = buffer[..streamLength];

                var json = Encoding.UTF8.GetString(buffer);

                _logger.Info(json);

                var packet = UserPacket.Deserialize(json) as JoinRequestPacket ??
                             throw new Exception("Could not deserialize packet!");

                if (_connectedClients.Any(i => i.UserInfo.UserId == packet.UserId))
                {
                    await client.GetStream()
                        .WriteAsync(new JoinResponsePacket(false, "You are logged in from another location!")
                            .SerializeToBytes());
                    client.Close();
                    continue;
                }

                var connectedClient = new ConnectedClient(client,
                    _userData.UpdateUserDataOnLogin(packet.UserId, packet.UserName), _logger);

                var targetMatchmaker = _queueManager.GetQueueFromName(packet.Queue);

                if (targetMatchmaker == null)
                {
                    await connectedClient.SendPacket(new JoinResponsePacket(false, "Invalid Queue"));
                    connectedClient.Disconnect();
                    continue;
                }

                await connectedClient.SendPacket(new JoinResponsePacket(true, "success"));

                targetMatchmaker.AddClientToPool(connectedClient);

                _connectedClients.Add(connectedClient);
                connectedClient.OnDisconnected += OnDisconnected;

                _logger.Info($"User {connectedClient.UserInfo.Username} ({connectedClient.UserInfo.UserId}) joined queue {targetMatchmaker.QueueName}");
            }
            catch (Exception e)
            {
                _logger.Error(e);
                client.Close();
            }
        }
    }

    private void OnDisconnected(IConnectedClient client)
    {
        client.OnDisconnected -= OnDisconnected;
        
        _connectedClients.Remove(client);
        _logger.Info($"{client.UserInfo.Username} ({client.UserInfo.UserId}) disconnected");
    }

    private void Stop()
    {
        _listener.Stop();
    }

    public void Dispose()
    {
        Stop();
    }
}