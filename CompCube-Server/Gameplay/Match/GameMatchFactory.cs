using CompCube_Server.Interfaces;

namespace CompCube_Server.Gameplay.Match;

public class GameMatchFactory(IServiceProvider services)
{
    public GameMatch CreateNewMatch(IConnectedClient playerOne, IConnectedClient playerTwo, MatchSettings settings)
    {
        var match = ActivatorUtilities.CreateInstance<GameMatch>(services);
        
        match.Init(playerOne, playerTwo, settings);

        return match;
    }
}