using CompCube_Models.Models.Match;
using CompCube_Models.Models.Packets.UserPackets;
using CompCube_Server.Interfaces;
using CompCube_Server.Logging;
using CompCube.Gameplay.Match;

namespace CompCube_Server.Gameplay.Match;

public class ScoreManager
{
    public event Action<TeamScores, TeamScores>? OnWinnerDetermined;

    private readonly Logger _logger;

    private readonly Team _redTeam;
    private readonly Team _blueTeam;

    private TeamScores? _firstTeamScore = null;
    
    public ScoreManager(Team redTeam, Team blueTeam, Logger logger)
    {
        _redTeam = redTeam;
        _blueTeam = blueTeam;
        
        _logger = logger;
        
        redTeam.HandleMatchStarting(OnTeamScoreSubmitted);
    }

    private void OnTeamScoreSubmitted(TeamScores scores)
    {
        if (_firstTeamScore == null)
        {
            _firstTeamScore = scores;
            return;
        }

        var winner = scores.TotalScore > _firstTeamScore.TotalScore ? scores : _firstTeamScore;

        var loser = winner == scores ? _firstTeamScore : scores;
        
        OnWinnerDetermined?.Invoke(winner, loser);
    }
}