using System.Diagnostics;
using System.Net.Sockets;
using LoungeSaber_Server.Models.Packets;
using LoungeSaber_Server.Models.Packets.ServerPackets;
using LoungeSaber_Server.Models.Packets.UserPackets;
using LoungeSaber_Server.SQL;
using Newtonsoft.Json;

namespace LoungeSaber_Server.Models.Client;

public class DummyConnectedClient()
    : ConnectedClient(null!, UserData.Instance.GetUserById("0") ?? throw new Exception())
{
    public override async Task SendPacket(ServerPacket packet)
    {
        Console.WriteLine($"Debug Recieved: {JsonConvert.SerializeObject(packet)}");
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
            case ServerPacket.ServerPacketTypes.PrematureMatchEnd:
                break;
            default:
                throw new Exception("Unknown packet type");
        }
    }
}