using System.Net.Sockets;
using System.Text;
using LoungeSaber_Server.Models.Packets;
using LoungeSaber_Server.Models.Packets.UserPackets;
using Newtonsoft.Json;

namespace LoungeSaber_Server.Models.Client;

public class ConnectedClient
{
    private readonly TcpClient _client;
    
    public readonly UserInfo UserInfo;

    private bool _listenToClient = true;
    
    public event Action<VotePacket, ConnectedClient>? OnUserVoted;
    
    public event Action<ScoreSubmissionPacket, ConnectedClient>? OnScoreSubmission;

    public ConnectedClient(TcpClient client, UserInfo userInfo)
    {
        _client = client;
        UserInfo = userInfo;

        if (this is DummyConnectedClient) 
            return;
        
        var listenerThread = new Thread(ListenToClient);
        listenerThread.Start();
    }

    private void ListenToClient()
    {
        try
        {
            while (_listenToClient)
            {
                if (!_client.Connected)
                {
                    _listenToClient = false;
                    return;
                }
                
                var buffer = new byte[1024];

                _client.GetStream().Flush();
                
                while (!_client.GetStream().DataAvailable);
                
                var bytesRead = _client.GetStream().Read(buffer, 0, buffer.Length);
                Array.Resize(ref buffer, bytesRead);
                
                var json = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();

                Console.WriteLine($"Recieved from client: {json}");
                
                var packet = UserPacket.Deserialize(json);

                ProcessRecievedPacket(packet);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            StopListeningToClient();
        }
    }

    protected void ProcessRecievedPacket(UserPacket packet)
    {
        Console.WriteLine("Sent to client: ");
        switch (packet.PacketType)
        {
            case UserPacket.UserPacketTypes.Vote:
                OnUserVoted?.Invoke(packet as VotePacket ?? throw new Exception("Could not parse vote packet!"), this);
                break;
            case UserPacket.UserPacketTypes.ScoreSubmission:
                OnScoreSubmission?.Invoke(packet as ScoreSubmissionPacket ?? throw new Exception("Could not parse score submission packet!"), this);
                break;
            default:
                StopListeningToClient();
                throw new Exception("Unknown packet type!");
        }
    }

    public virtual async Task SendPacket(ServerPacket packet)
    {
        Console.WriteLine($"Sent to {UserInfo.Username}: " + JsonConvert.SerializeObject(packet));
        await _client.GetStream().WriteAsync(packet.SerializeToBytes());
    }

    public void StopListeningToClient()
    {
        _listenToClient = false;
        _client.Close();
    }
}