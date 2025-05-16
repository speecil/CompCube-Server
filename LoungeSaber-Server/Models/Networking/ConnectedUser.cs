using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using LoungeSaber_Server.Models.Maps;
using LoungeSaber_Server.SQL;

namespace LoungeSaber_Server.Models.Networking;

public class ConnectedUser
{
    public readonly User UserInfo;
    private readonly TcpClient _client;

    public bool IsInMatch { get; private set; } = false;
    private bool ShouldContinueReadingStream { get; set; } = true;

    public event Action<User, VotingMap>? OnUserVoteRecieved;
    public event Action<User, int>? OnUserScorePosted;
    public event Action<User>? OnUserLeftGame;

    public ConnectedUser(User user, TcpClient client)
    {
        UserInfo = user;
        _client = client;

        var clientStreamReaderThread = new Thread(GetClientActionFromStream);
        clientStreamReaderThread.Start();
    }

    private void GetClientActionFromStream()
    {
        while (ShouldContinueReadingStream)
        {
            var buffer = new byte[1024];
            
            var bufferLength = _client.GetStream().Read(buffer);
            buffer = buffer[..bufferLength];

            var json = Encoding.UTF8.GetString(buffer);
            var userAction = UserAction.Parse(json);

            switch (userAction.Type)
            {
                case UserAction.ActionType.VoteOnMap:
                    if (!userAction.JsonData.TryGetValue("vote", out var voteToken))
                        throw new Exception("Could not parse vote from client request!");
                    
                    OnUserVoteRecieved?.Invoke(UserInfo, voteToken.ToObject<VotingMap>()!);
                    break;
                case UserAction.ActionType.PostScore:
                    if (!userAction.JsonData.TryGetValue("score", out var score))
                    {
                        _client.Close();
                        continue;
                    }
                        
                    OnUserScorePosted?.Invoke(UserInfo, score.ToObject<int>());
                    break;
                case UserAction.ActionType.Leave:
                    OnUserLeftGame?.Invoke(userAction.User);
                    break;
            }
        }
    }

    public async Task SendServerAction(ServerAction action) =>
        await _client.GetStream().WriteAsync(Encoding.UTF8.GetBytes(action.Serialize()));
}