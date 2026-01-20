using System.Net.Sockets;
using System.Text;
using CompCube_Models.Models.ClientData;
using CompCube_Models.Models.Packets;
using CompCube_Models.Models.Packets.UserPackets;
using CompCube_Server.Interfaces;
using CompCube_Server.Logging;

namespace CompCube_Server.Networking.Client;

public class ConnectedClient : IConnectedClient, IDisposable
{
    private readonly Logger _logger;
    
    private readonly TcpClient _client;

    private bool _listenToClient = true;
    
    public event Action<VotePacket, IConnectedClient>? OnUserVoted;
    public event Action<ScoreSubmissionPacket, IConnectedClient>? OnScoreSubmission;
    public event Action<IConnectedClient>? OnDisconnected;

    public UserInfo UserInfo { get; }

    public ConnectedClient(TcpClient client, UserInfo userInfo, Logger logger)
    {
        _client = client;
        UserInfo = userInfo;
        _logger = logger;

        Task.Factory.StartNew(ListenToClient, TaskCreationOptions.LongRunning);
    }

    private async Task ListenToClient()
    {
        while (_listenToClient)
        {
            try
            {
                if (!IsConnectionAlive)
                {
                    Disconnect();
                    return;
                }

                var buffer = new byte[1024];

                if (IsConnectionAlive)
                    _client.GetStream().Flush();
                
                if (!_client.GetStream().DataAvailable)
                    continue;
                
                var bytesRead = _client.GetStream().Read(buffer, 0, buffer.Length);
                Array.Resize(ref buffer, bytesRead);
                
                var json = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();

                if (json.Length == 0)
                    continue;

                var packet = UserPacket.Deserialize(json);

                ProcessRecievedPacket(packet);
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            catch (Exception e)
            {
                Disconnect();
                _logger.Error(e);
            }
        }
    }

    public void Disconnect()
    {
        _listenToClient = false;
        _client.Close();
        
        OnDisconnected?.Invoke(this);
    }

    public bool IsConnectionAlive
    {
        get
        {
            try
            {
                var poll = _client.Client.Poll(1, SelectMode.SelectRead) && !_client.GetStream().DataAvailable;

                return !poll;
            }
            catch (Exception e)
            {
                if (e is SocketException socketException)
                    return socketException.SocketErrorCode is SocketError.WouldBlock or SocketError.Interrupted;

                return false;
            }
        }
    }

    private void ProcessRecievedPacket(UserPacket packet)
    {
        switch (packet.PacketType)
        {
            case UserPacket.UserPacketTypes.Vote:
                OnUserVoted?.Invoke(packet as VotePacket ?? throw new Exception("Could not parse vote packet!"), this);
                break;
            case UserPacket.UserPacketTypes.ScoreSubmission:
                OnScoreSubmission?.Invoke(packet as ScoreSubmissionPacket ?? throw new Exception("Could not parse score submission packet!"), this);
                break;
            default:
                Disconnect();
                throw new Exception("Unknown packet type!");
        }
    }

    public async Task SendPacket(ServerPacket packet)
    {
        await _client.GetStream().WriteAsync(packet.SerializeToBytes());
        
        // wait a tenth of a second to prevent packets from being sent in the same time frame and being read as
        // one super long packet
        
        // this value may need to be decreased in the future
        await Task.Delay(50);
    }

    public void Dispose()
    {
        Disconnect();
    }
}