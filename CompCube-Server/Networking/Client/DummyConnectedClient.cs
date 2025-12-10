using CompCube_Models.Models.ClientData;
using CompCube_Models.Models.Packets;
using CompCube_Models.Models.Packets.UserPackets;
using CompCube_Server.Interfaces;
using VotePacket = CompCube_Models.Models.Packets.UserPackets.VotePacket;

namespace CompCube_Server.Networking.Client;

public class DummyConnectedClient(UserInfo userInfo) : IConnectedClient
{
    public event Action<VotePacket, IConnectedClient>? OnUserVoted;
    public event Action<ScoreSubmissionPacket, IConnectedClient>? OnScoreSubmission;
    public event Action<IConnectedClient>? OnDisconnected;

    public UserInfo UserInfo => userInfo;

    public Task SendPacket(ServerPacket packet)
    {
        switch (packet.PacketType)
        {
            case ServerPacket.ServerPacketTypes.OpponentVoted:
                OnUserVoted?.Invoke(new VotePacket(0), this);
                break;
            case ServerPacket.ServerPacketTypes.BeginGameTransition:
                OnScoreSubmission?.Invoke(new ScoreSubmissionPacket(10000, 10000, true, 0, true), this);
                break;
            default:
                break;
        }

        return Task.CompletedTask;
    }

    public void Disconnect()
    {
        OnDisconnected?.Invoke(this);
    }
}