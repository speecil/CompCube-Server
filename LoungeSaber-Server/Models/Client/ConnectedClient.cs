using System.Net.Sockets;
using System.Text;
using LoungeSaber_Server.Models.Packets;

namespace LoungeSaber_Server.Models.Client;

public class ConnectedClient
{
    private readonly TcpClient _client;
    
    public readonly UserInfo UserInfo;

    private bool _listenToClient = true;
    
    public event Action<UserPacket> OnPacketReceived;

    public ConnectedClient(TcpClient client, UserInfo userInfo)
    {
        _client = client;
        UserInfo = userInfo;
        
        var listenerThread = new Thread(ListenToClient);
        listenerThread.Start();
    }

    private void ListenToClient()
    {
        try
        {
            while (_client.Connected)
            {
                var buffer = new byte[1024];

                while (!_client.GetStream().DataAvailable);
                
                var bytesRead = _client.GetStream().Read(buffer, 0, buffer.Length);
                Array.Resize(ref buffer, bytesRead);

                var packet = UserPacket.Deserialize(Encoding.UTF8.GetString(buffer));
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public virtual async Task SendPacket(ServerPacket packet)
    {
        await _client.GetStream().WriteAsync(packet.SerializeToBytes());
    }

    public void StopListeningToClient()
    {
        _listenToClient = false;
        _client.Close();
    }
}