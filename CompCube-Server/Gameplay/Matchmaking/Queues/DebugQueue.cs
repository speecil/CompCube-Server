using CompCube_Models.Models.ClientData;
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
        var match = gameMatchFactory.CreateNewMatch([client], [new DummyConnectedClient(new UserInfo("debug", "0", 1000, new DivisionInfo(DivisionInfo.DivisionName.Bronze, 1, "#000000", false), null, 0, null, false, 0, 0, 0, 0))], new MatchSettings(false, false, 0, 0));
        match.StartMatch();
    }
}