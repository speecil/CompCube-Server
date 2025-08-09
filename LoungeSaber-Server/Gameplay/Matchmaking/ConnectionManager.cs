using System.Net;
using System.Net.Sockets;
using System.Text;
using LoungeSaber_Server.Models.Client;
using LoungeSaber_Server.Models.Packets;
using LoungeSaber_Server.Models.Packets.ServerPackets;
using LoungeSaber_Server.Models.Packets.UserPackets;
using LoungeSaber_Server.SQL;

namespace LoungeSaber_Server.Gameplay.Matchmaking;

public class ConnectionManager : IDisposable
{
    private readonly UserData _userData;
    
    //TODO: change listener ip when not developing
    private readonly TcpListener _listener = new(IPAddress.Any, 8008);

    private readonly Thread _listenForClientsThread;
    
    private bool _isStarted = false;

    public event Action<ConnectedClient>? OnClientJoined;

    public ConnectionManager(UserData userData)
    {
        _userData = userData;
        
        _listenForClientsThread = new Thread(ListenForClients);
        Start();
    }
    
    private void Start()
    {
        _listener.Start();
        _isStarted = true;
        
        _listenForClientsThread.Start();
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
                    Console.WriteLine(json);

                    var packet = UserPacket.Deserialize(json) as JoinRequestPacket ?? throw new Exception("Could not deserialize packet!");

                    var connectedClient = new ConnectedClient(client, _userData.UpdateUserDataOnLogin(packet.UserId, packet.UserName));

                    await connectedClient.SendPacket(new JoinResponsePacket(true, ""));
                    
                    OnClientJoined?.Invoke(connectedClient);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    client.Close();
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public void Stop()
    {
        _isStarted = false;
        _listener.Dispose();
    }

    public void Dispose()
    {
        Stop();
    }
}