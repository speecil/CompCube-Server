using CompCube_Models.Models.ClientData;
using CompCube_Models.Models.Match;

namespace CompCube_Server.Interfaces;

public interface IDiscordBot
{
    public void PostMatchResults(MatchResultsData matchResults);

    public void PostEventScores(List<MatchScore> scores, List<UserInfo> usersWithoutScores);

    public void PostEventPoints(Dictionary<UserInfo, int> points);
}