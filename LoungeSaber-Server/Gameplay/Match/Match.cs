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
        var randomMapSelections = GetRandomMapSelections(3);
        
        
    }

    private VotingMap[] GetRandomMapSelections(int amount)
    {
        var random = new Random();
        
        var selections = new List<VotingMap>();
        
        var allMaps = MapData.Instance.GetAllMaps();

        while (selections.Count < amount)
        {
            var randomMap = allMaps[random.Next(0, allMaps.Count)];
            
            if (selections.Any(i => i.Category == randomMap.Category)) continue;
            if (selections.Any(i => i.Hash == randomMap.Hash)) continue;
            
            selections.Add(randomMap);
        }

        return selections.ToArray();
    }
}