using CompCube_Models.Models.Match;
using CompCube_Server.Discord.Events;
using CompCube_Server.Gameplay.Match;
using CompCube_Server.Interfaces;
using CompCube_Server.Logging;
using CompCube_Server.Models.Client;
using CompCube_Server.SQL;

namespace CompCube_Server.Gameplay.Matchmaking;

public class StandardCasualQueue(GameMatchFactory gameMatchFactory) : StandardQueue
{
    private readonly List<MatchmakingClient> _clientPool = [];

    public override string QueueName => "standard_casual_1v1";

    public override void AddClientToPool(IConnectedClient client)
    {
        _clientPool.Add(new MatchmakingClient(client));

        if (_clientPool.Count != 2)
            return;
        
        var playerOne = _clientPool[0];
        var playerTwo = _clientPool[1];

        var match = gameMatchFactory.CreateNewMatch(playerOne.Client, playerTwo.Client, new MatchSettings(true, false));
        match.StartMatch();
    }
}