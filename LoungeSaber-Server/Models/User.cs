namespace LoungeSaber_Server.Models;

public class User
{
    public string ID { get; private set; }
    public int MMR { get; private set; }

    public User(string id, int mmr)
    {
        ID = id;
        MMR = mmr;
    }
}