using CompCube_Server.Interfaces;

namespace CompCube_Server.Gameplay.Match;

public class GameMatchFactory(IServiceProvider services)
{
    public GameMatch CreateNewMatch(IConnectedClient[] redTeamPlayers, IConnectedClient[] blueTeamPlayers, MatchSettings settings)
    {
        var match = ActivatorUtilities.CreateInstance<GameMatch>(services);
        
        match.Init(redTeamPlayers, blueTeamPlayers, settings);

        return match;
    }
}