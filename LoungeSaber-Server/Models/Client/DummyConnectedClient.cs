using System.Diagnostics;
using System.Net.Sockets;
using LoungeSaber_Server.Models.Packets;
using LoungeSaber_Server.Models.Packets.ServerPackets;
using LoungeSaber_Server.Models.Packets.UserPackets;

namespace LoungeSaber_Server.Models.Client;

public class DummyConnectedClient()
    : ConnectedClient(null!, new UserInfo("debug", "0", 1000, new Badge.Badge("dummy", "#808080", true)))
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
                var matchStartedPacket = packet as MatchStarted ?? throw new Exception("no way this is a valid thing that can happen lmfao");

                while (matchStartedPacket.StartingTime > DateTime.UtcNow);
                
                ProcessRecievedPacket(new ScoreSubmissionPacket(100000, 100000, true, 0, true));
                break;
            case ServerPacket.ServerPacketTypes.MatchResults:
                break;
            default:
                throw new Exception("Unknown packet type");
        }
    }
}