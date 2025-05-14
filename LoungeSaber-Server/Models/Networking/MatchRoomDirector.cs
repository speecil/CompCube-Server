using System.Net;
using System.Net.Sockets;
using System.Text;
using LoungeSaber_Server.SkillDivision;

namespace LoungeSaber_Server.Models.Networking;

public static class MatchRoomDirector
{
    private static readonly TcpListener _listener = new(IPAddress.Parse("127.0.0.1"), 8008);

    private static bool IsStarted = false;

    public static async Task Start()
    {
        _listener.Start();
        IsStarted = true;
        
        while (IsStarted)
        {
            var client = await _listener.AcceptTcpClientAsync();

            var buffer = new byte[1024];

            await client.GetStream().ReadAsync(buffer, 0, buffer.Length);
            var roomRequest = UserAction.Parse(Encoding.UTF8.GetString(buffer));
            
            if (roomRequest.Type != UserAction.ActionType.Join)
            {
                client.Close();
                continue;
            }
            
            
        }
    }

    public static void Stop() => IsStarted = false;
}