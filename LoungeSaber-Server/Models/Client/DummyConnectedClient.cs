using System.Net.Sockets;
using LoungeSaber_Server.Models.Packets;

namespace LoungeSaber_Server.Models.Client;

public class DummyConnectedClient()
    : ConnectedClient(null!, new UserInfo("debug", "0", 1000, new Badge.Badge("dummy", "#808080", false)))
{
    public override Task SendPacket(ServerPacket packet)
    {
        switch (packet.PacketType)
        {
            case ServerPacket.ServerPacketTypes.MatchCreated:
                
                break;
            default:
                throw new Exception("Unknown packet type");
        }
        
        return Task.CompletedTask;
    }
}