using LoungeSaber_Server.Gameplay.Match;
using LoungeSaber_Server.Models.Client;

namespace LoungeSaber_Server.Interfaces;

public interface IMatchmaker
{
    public event Action<Match>? OnMatchStarted;
    
    public void AddClientToPool(ConnectedClient client);
}