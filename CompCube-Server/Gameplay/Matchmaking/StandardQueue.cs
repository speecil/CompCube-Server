using CompCube_Server.Interfaces;

namespace CompCube_Server.Gameplay.Matchmaking;

public abstract class StandardQueue : IQueue
{
    public abstract string QueueName { get; }

    public abstract void AddClientToPool(IConnectedClient client);
}