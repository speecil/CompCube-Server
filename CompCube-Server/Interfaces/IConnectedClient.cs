using CompCube_Models.Models.ClientData;
using CompCube_Models.Models.Packets;
using CompCube_Models.Models.Packets.UserPackets;

namespace CompCube_Server.Interfaces;

public interface IConnectedClient
{
    public event Action<VotePacket, IConnectedClient>? OnUserVoted;
    
    public event Action<ScoreSubmissionPacket, IConnectedClient>? OnScoreSubmission;
    
    public event Action<IConnectedClient>? OnDisconnected;

    public UserInfo UserInfo { get; }

    public Task SendPacket(ServerPacket packet);

    public void Disconnect();
}