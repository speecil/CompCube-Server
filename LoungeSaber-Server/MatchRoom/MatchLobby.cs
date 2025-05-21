using System.Globalization;
using System.Timers;
using LoungeSaber_Server.Models;
using LoungeSaber_Server.Models.Divisions;
using LoungeSaber_Server.Models.Networking;
using Newtonsoft.Json.Linq;
using Timer = System.Timers.Timer;

namespace LoungeSaber_Server.MatchRoom;

public class MatchLobby
{
    public readonly Division Division;
    
    public List<ConnectedUser> ConnectedUsers = [];

    private List<Match> InProgressMatches = [];

    public bool MatchesInProgress => InProgressMatches.Count > 0;

    private Timer _canStartNewMatchesTimer = new Timer
    {
        Enabled = false,
        AutoReset = false,
        Interval = 30000
    };

    private bool _didMatchTimerRunOut = false;
    
    public MatchLobby(Division division)
    {
        Division = division;
    }
    
    private bool CanJoinRoom(int mmr) => mmr >= Division.MinMMR && (Division.MaxMMR == 0 || mmr <= Division.MaxMMR);

    public async Task<bool> JoinRoom(ConnectedUser user)
    {
        if (!CanJoinRoom(user.UserInfo.MMR)) 
            return false;
        
        ConnectedUsers.Add(user);
        await SendToClients(new ServerPacket(ServerPacket.ActionType.UpdateConnectedUserCount, new JObject
        {
            {"userCount", ConnectedUsers.Count}
        }));
        
        return true;
    }
    
    private bool CanStartMatch => ConnectedUsers.Count % 2 == 0 && ConnectedUsers.Count > 0 && !MatchesInProgress && _didMatchTimerRunOut;

    private async Task StartMatches()
    {
        if (MatchesInProgress) return;
        if (!CanStartMatch) return;

        var startingTime = DateTime.UtcNow.Add(new TimeSpan(0, 0, 0, 5));

        var startingMatchServerAction = new ServerPacket(ServerPacket.ActionType.StartWarning, new JObject()
        {
            {"startTime", startingTime.ToString("o", CultureInfo.InvariantCulture)}
        });

        await SendToClients(startingMatchServerAction);
        
        await Task.Delay((int) startingTime.Subtract(DateTime.UtcNow).TotalMilliseconds);

        InProgressMatches = CreateMatches().ToList();
    }

    private Match[] CreateMatches()
    {
        var users = ConnectedUsers.ToList();
        var random = new Random();

        var matches = new List<Match>();
        
        while (users.Count > 0)
        {
            var randomUserOne = users[random.Next(0, users.Count + 1)];
            users.Remove(randomUserOne);

            var randomUserTwo = users[random.Next(0, users.Count + 1)];
            users.Remove(randomUserTwo);

            var match = new Match(randomUserOne, randomUserTwo, Division);
            match.MatchEnded += OnMatchEnded;
            matches.Add(match);
        }

        return matches.ToArray();
    }

    private void OnMatchEnded(Match match)
    {
        match.MatchEnded -= OnMatchEnded;
        InProgressMatches.Remove(match);

        if (MatchesInProgress) 
            return;
        
        _canStartNewMatchesTimer.Elapsed += OnCanStartNewMatches;
    }

    private async void OnCanStartNewMatches(object? sender, ElapsedEventArgs _)
    {
        try
        {
            _didMatchTimerRunOut = true;

            if (!CanStartMatch) 
                return;

            await StartMatches();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private async Task SendToClients(ServerPacket packet)
    {
        var users = ConnectedUsers;
        
        foreach (var client in users)
            await client.SendServerPacket(packet);
    }
}