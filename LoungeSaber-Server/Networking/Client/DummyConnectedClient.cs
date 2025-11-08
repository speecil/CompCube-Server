using LoungeSaber_Server.Interfaces;
using LoungeSaber_Server.Logging;
using LoungeSaber_Server.Models.ClientData;
using LoungeSaber_Server.Models.Packets;
using LoungeSaber_Server.Models.Packets.ServerPackets;
using LoungeSaber_Server.Models.Packets.UserPackets;
using Newtonsoft.Json;

namespace LoungeSaber_Server.Models.Client;

public class DummyConnectedClient(UserInfo userInfo) : IConnectedClient
{
    public event Action<VotePacket, IConnectedClient>? OnUserVoted;
    public event Action<ScoreSubmissionPacket, IConnectedClient>? OnScoreSubmission;
    public event Action<IConnectedClient>? OnDisconnected;

    public UserInfo UserInfo => userInfo;

    public async Task SendPacket(ServerPacket packet)
    {
        switch (packet.PacketType)
        {
            case ServerPacket.ServerPacketTypes.MatchCreated:
                await Task.Delay(5000);
                OnUserVoted?.Invoke(new VotePacket(0), this);
                break;
            case ServerPacket.ServerPacketTypes.OpponentVoted:
                break;
            case ServerPacket.ServerPacketTypes.MatchStarted:
                var matchStartedPacket = packet as MatchStartedPacket ?? throw new Exception("no way this is a valid thing that can happen lmfao");

                while (DateTime.UtcNow.AddSeconds(matchStartedPacket.StartingWait) > DateTime.UtcNow);
                
                OnScoreSubmission?.Invoke(new ScoreSubmissionPacket(90000, 100000, true, 0, true), this);
                break;
            case ServerPacket.ServerPacketTypes.MatchResults:
                break;
            case ServerPacket.ServerPacketTypes.PrematureMatchEnd:
                break;
            default:
                throw new Exception("Unknown packet type");
        }
    }

    public void Disconnect()
    {
        OnDisconnected?.Invoke(this);
    }
}