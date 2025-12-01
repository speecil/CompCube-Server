namespace CompCube_Server.Gameplay.Match;

public class MatchSettings(bool logMatch, bool competitive, int kFactor, int mmrPenaltyOnDisconnect)
{
    public readonly bool LogMatch = logMatch;
    
    public readonly bool Competitive = competitive;
    
    public readonly int KFactor = kFactor;
    
    public readonly int MmrPenaltyOnDisconnect = mmrPenaltyOnDisconnect;
}