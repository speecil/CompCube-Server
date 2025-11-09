using CompCube_Models.Models.Match;
using CompCube_Server.Gameplay.Match;

namespace CompCube_Server.Interfaces;

public interface IQueue
{
    public string QueueName { get; }

    public event Action<MatchResultsData, Match> QueueMatchEnded;
    
    public void AddClientToPool(IConnectedClient client);
}