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
        
        var listenerThread = new Thread(ListenToClient);
        listenerThread.Start();
    }

    private void ListenToClient()
    {
        try
        {
            while (_listenToClient)
            {
                if (!IsConnectionAlive)
                {
                    Disconnect();
                    _logger.Info($"{UserInfo.Username} ({UserInfo.UserId}) disconnected");
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
                
                var packet = UserPacket.Deserialize(json);

                ProcessRecievedPacket(packet);
            }
        }
        catch (Exception e)
        {
            _logger.Error(e);
            Disconnect();
        }
    }

    public void Disconnect()
    {
        _listenToClient = false;
        _client.Close();
        
        OnDisconnected?.Invoke(this);
    }

    private bool IsConnectionAlive
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
    }

    public void Dispose()
    {
        Disconnect();
    }
}