using CompCube_Models.Models.Match;
using CompCube_Server.Gameplay.Match;

namespace CompCube.Gameplay.Match;

public class TeamScores(Team.TeamName team, MatchScore[] scores)
{
    public readonly Team.TeamName Team = team;

    public readonly MatchScore[] Scores = scores;

    public readonly int TotalScore = scores.Sum(i => i.Score?.Points) ?? 0;
}