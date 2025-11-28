using CompCube_Server.Interfaces;

namespace CompCube_Server.Gameplay.Match;

public class GameMatch
{
    private MatchSettings _matchSettings;

    private readonly Dictionary<IConnectedClient, Team> _teams = new();

    private readonly Dictionary<Team, int> _points = new();
    
    public void Init(IConnectedClient[] red, IConnectedClient[] blue, MatchSettings settings)
    {
        _matchSettings = settings;
        
        SetupPlayer(red, Team.Red);
        SetupPlayer(blue, Team.Blue);
        
        return;
        
        void SetupPlayer(IConnectedClient[] players, Team team)
        {
            foreach (var player in players)
            {
                player.OnDisconnected += HandleClientDisconnect;
                
                _teams.Add(player, team);
            }
        }
    }

    private void HandleClientDisconnect(IConnectedClient client)
    {
        client.OnDisconnected -= HandleClientDisconnect;

        _teams.Remove(client);
    }

    public enum Team
    {
        Red,
        Blue
    }
}