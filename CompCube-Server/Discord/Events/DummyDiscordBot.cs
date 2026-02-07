using CompCube_Models.Models.ClientData;
using CompCube_Models.Models.Match;
using CompCube_Server.Interfaces;

namespace CompCube_Server.Discord.Events;

public class DummyDiscordBot : IDiscordBot
{
    public void PostMatchResults(MatchResultsData matchResults)
    {
    }

    public void PostEventScores(List<MatchScore> scores, List<UserInfo> usersWithoutScores)
    {
    }

    public void PostEventPoints(Dictionary<UserInfo, int> points)
    {
    }
    
    
}