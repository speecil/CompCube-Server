using CompCube_Server.Discord.Events;
using CompCube_Server.Gameplay.Match;
using CompCube_Server.Interfaces;
using CompCube_Server.Logging;
using CompCube_Server.Networking.Client;
using CompCube_Server.SQL;

namespace CompCube_Server.Gameplay.Matchmaking;

public class DebugQueue(Logger logger, GameMatchFactory gameMatchFactory, UserData userData) : IQueue
{
    public string QueueName => "debug";

    public void AddClientToPool(IConnectedClient client)
    {
        var match = gameMatchFactory.CreateNewMatch([client], [new DummyConnectedClient(userData.GetUserById("0") ?? throw new Exception("Could not find debug user data!"))], new MatchSettings(false, false));
        match.StartMatch();
    }
}