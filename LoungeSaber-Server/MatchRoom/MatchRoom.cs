using LoungeSaber_Server.Models.Networking;
using LoungeSaber_Server.SkillDivision;

namespace LoungeSaber_Server.MatchRoom;

public class MatchRoom
{
    public readonly Division Division;
    
    public List<ConnectedUser> ConnectedUsers = [];
    
    public MatchRoom(Division division)
    {
        Division = division;
    }
    
    public bool CanJoinRoom(int mmr) => mmr >= Division.MinMMR && mmr < Division.MaxMMR;

    public bool JoinRoom(ConnectedUser user)
    {
        if (!CanJoinRoom(user.UserInfo.MMR)) return false;
        
        ConnectedUsers.Add(user);
        return true;
    }
}