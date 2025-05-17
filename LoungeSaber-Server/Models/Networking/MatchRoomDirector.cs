using System.Net;
using System.Net.Sockets;
using System.Text;
using LoungeSaber_Server.MatchRoom;
using LoungeSaber_Server.SkillDivision;
using LoungeSaber_Server.SQL;

namespace LoungeSaber_Server.Models.Networking;

public static class MatchRoomDirector
{
    private static readonly TcpListener Listener = new(IPAddress.Parse("127.0.0.1"), 8008);

    private static bool IsStarted = false;

    public static async Task Start()
    {
        Listener.Start();
        IsStarted = true;
        
        while (IsStarted)
        {
            var client = await Listener.AcceptTcpClientAsync();
            
            try
            {
                var buffer = new byte[1024];

                var streamLength = await client.GetStream().ReadAsync(buffer, 0, buffer.Length);
                buffer = buffer[..streamLength];
                
                var roomRequest = UserAction.Parse(Encoding.UTF8.GetString(buffer));

                if (roomRequest.Type != UserAction.ActionType.Join ||
                    !roomRequest.JsonData.TryGetValue("divisionName", out var divisionName) ||
                    !DivisionManager.TryGetDivisionFromName(divisionName.ToObject<string>()!, out var division) || 
                    !roomRequest.JsonData.TryGetValue("userId", out var userId))
                {
                    client.Close();
                    continue;
                }

                if (!UserData.Instance.TryGetUserById(userId.ToObject<string>()!, out var user))
                    UserData.Instance.AddNewUserToDatabase(userId.ToObject<string>()!, out user);

                if (user == null)
                {
                    client.Close();
                    continue;
                }

                if (!division?.DivisionLobby.JoinRoom(new ConnectedUser(user, client)) ?? false)
                    continue;
                
                client.Close();
            }
            catch (Exception e)
            {
                Program.LogError(e.Message);
                client.Close();
            }
        }
    }

    public static void Stop()
    {
        IsStarted = false;
        Listener.Stop();
    }
}