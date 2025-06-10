using LoungeSaber_Server.Models.Client;

namespace LoungeSaber_Server.Gameplay.Matchmaking;

public class DebugMatchmaker : Matchmaker
{
    public static readonly DebugMatchmaker DebugInstance = new();

    public override void AddClientToPool(ConnectedClient client)
    {
        var match = new Match.Match(client, null!);
    }
}