using CompCube_Server.Interfaces;

namespace CompCube_Server.Gameplay.Events;

public class BracketManager
{
    
    
    public BracketManager(List<IConnectedClient> clients)
    {
        var bracketOrder = clients.OrderBy(i => i.UserInfo.Mmr);
    }
}