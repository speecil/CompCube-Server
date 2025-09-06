using LoungeSaber_Server.Logging;
using LoungeSaber_Server.Models.ClientData;
using LoungeSaber_Server.Models.Packets;
using LoungeSaber_Server.Models.Packets.ServerPackets;
using LoungeSaber_Server.Models.Packets.UserPackets;
using Newtonsoft.Json;

namespace LoungeSaber_Server.Models.Client;

public class DummyConnectedClient(UserInfo userInfo, Logger logger)
    : ConnectedClient(null!, userInfo, logger)
{
    public override async Task SendPacket(ServerPacket packet)
    {
        switch (packet.PacketType)
        {
            case ServerPacket.ServerPacketTypes.MatchCreated:
                await Task.Delay(5000);
                ProcessRecievedPacket(new VotePacket(0));
                break;
            case ServerPacket.ServerPacketTypes.OpponentVoted:
                break;
            case ServerPacket.ServerPacketTypes.MatchStarted:
                var matchStartedPacket = packet as MatchStartedPacket ?? throw new Exception("no way this is a valid thing that can happen lmfao");

                while (matchStartedPacket.StartingTime > DateTime.UtcNow);
                
                ProcessRecievedPacket(new ScoreSubmissionPacket(90000, 100000, true, 0, true));
                break;
            case ServerPacket.ServerPacketTypes.MatchResults:
                break;
            case ServerPacket.ServerPacketTypes.PrematureMatchEnd:
                break;
            default:
                throw new Exception("Unknown packet type");
        }
    }

    public override void Disconnect() {}

    protected override void ListenToClient() {}
}