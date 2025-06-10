using LoungeSaber_Server.Models.Client;
using LoungeSaber_Server.Models.Map;
using LoungeSaber_Server.SQL;

namespace LoungeSaber_Server.Gameplay.Match;

public class Match(ConnectedClient playerOne, ConnectedClient playerTwo)
{
    public readonly ConnectedClient PlayerOne = playerOne;
    public readonly ConnectedClient PlayerTwo = playerTwo;

    public void StartMatch()
    {
        var randomMapSelections = new List<VotingMap>();
    }

    private VotingMap[] GetRandomMapSelections(int amount)
    {
        var selections = new List<VotingMap>();
        
        var allMaps = MapData.Instance.

        while (selections.Count < amount)
        {
            
        }

        return selections.ToArray();
    }
}