using CompCube_Models.Models.Match;
using CompCube_Server.Discord.Events;
using CompCube_Server.Gameplay.Match;
using CompCube_Server.Interfaces;
using CompCube_Server.Logging;
using CompCube_Server.Models.Client;
using CompCube_Server.Networking.Client;
using CompCube_Server.SQL;

namespace CompCube_Server.Gameplay.Matchmaking;

public class StandardCasualQueue(GameMatchFactory gameMatchFactory) : StandardQueue
{
    private IConnectedClient? _otherClient = null;

    public override string QueueName => "standard_casual_1v1";

    public override void AddClientToPool(IConnectedClient client)
    {
        if (_otherClient == null)
        {
            _otherClient = client;
            
            _otherClient.OnDisconnected += HandleClientDisconnect;
            
            return;
        }

        var match = gameMatchFactory.CreateNewMatch([client], [_otherClient], new MatchSettings(true, false, 0, 0));

        _otherClient.OnDisconnected -= HandleClientDisconnect;
        _otherClient = null;
        
        Task.Run(() =>
        {
            match.StartMatch();
        });
    }

    private void HandleClientDisconnect(IConnectedClient client)
    {
        client.OnDisconnected -= HandleClientDisconnect;

        _otherClient = null;
    }
}