namespace CompCube_Server.Gameplay.Match;

public class MatchSettings(bool logMatch, bool competitive)
{
    public readonly bool LogMatch = logMatch;
    
    public readonly bool Competitive = competitive;
}