using System.Net;
using System.Net.Sockets;
using System.Text;
using LoungeSaber_Server.Models.Client;
using LoungeSaber_Server.Models.Packets;
using LoungeSaber_Server.Models.Packets.ServerPackets;
using LoungeSaber_Server.Models.Packets.UserPackets;
using LoungeSaber_Server.SQL;

namespace LoungeSaber_Server.Gameplay.Matchmaking;

public static class ConnectionManager
{
    //TODO: change listener ip when not developing
    private static readonly TcpListener Listener = new(IPAddress.Parse("192.168.1.6"), 8008);

    private static readonly Thread ListenForClientsThread = new(ListenForClients);
    
    private static bool _isStarted = false;

    public static void Start()
    {
        Listener.Start();
        _isStarted = true;
        
        ListenForClientsThread.Start();
    }

    private static async void ListenForClients()
    {
        try
        {
            while (_isStarted)
            {
                var client = Listener.AcceptTcpClient();
                
                try
                {
                    var buffer = new byte[1024];

                    var streamLength = client.GetStream().Read(buffer, 0, buffer.Length);
                    buffer = buffer[..streamLength];

                    var json = Encoding.UTF8.GetString(buffer);
                    Console.WriteLine(json);

                    var packet = UserPacket.Deserialize(json) as JoinRequestPacket ?? throw new Exception("Could not deserialize packet!");

                    var connectedClient = new ConnectedClient(client, UserData.Instance.UpdateUserDataOnLogin(packet.UserId, packet.UserName));

                    await connectedClient.SendPacket(new JoinResponse(true, ""));
                    
                    await Matchmaker.AddClientToPool(connectedClient);
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

    public static void Stop()
    {
        _isStarted = false;
        Listener.Stop();
    }
}