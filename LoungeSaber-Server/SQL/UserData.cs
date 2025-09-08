using System.Data;
using System.Data.SQLite;
using LoungeSaber_Server.Models.Client;
using LoungeSaber_Server.Models.ClientData;

namespace LoungeSaber_Server.SQL;

public class UserData : Database
{
    protected override string DatabaseName => "LoungeData";

    public UserInfo? GetUserByDiscordId(string discordId)
    {
        var command = Connection.CreateCommand();
        command.CommandText = "SELECT * FROM userData WHERE discordId = @discordId LIMIT 1";
        command.Parameters.AddWithValue("discordId", discordId);
        
        using var reader = command.ExecuteReader();

        while (reader.Read())
            return GetUserInfoFromReader(reader);
        
        return null;
    }

    public void LinkDiscordToUser(string userId, string discordId)
    {
        var command = Connection.CreateCommand();
        command.CommandText = "UPDATE userData SET discordId = @discordId WHERE id = @userId";
        command.Parameters.AddWithValue("userId", userId);
        command.Parameters.AddWithValue("discordId", discordId);

        command.ExecuteNonQuery();
    }

    public UserInfo? GetUserById(string userId)
    {
        var command = Connection.CreateCommand();
        command.CommandText = "SELECT * FROM userData WHERE userData.id = @id LIMIT 1";
        command.Parameters.AddWithValue("id", userId);
        using var reader = command.ExecuteReader();

        while (reader.Read())
            return GetUserInfoFromReader(reader);
        
        return null;
    }

    private UserInfo? GetUserInfoFromReader(SQLiteDataReader reader)
    {
        if (reader.FieldCount == 0) 
            return null;
            
        var userId = reader.GetString(0);
        var mmr = reader.GetInt32(1);
        var userName = reader.GetString(2);
        Badge? badge = null;
        string? discordId = null;

        var rankCommand = Connection.CreateCommand();
        rankCommand.CommandText = $"SELECT COUNT(*) FROM userData WHERE mmr > @mmrThreshold ORDER BY mmr";
        rankCommand.Parameters.AddWithValue("mmrThreshold", mmr);
        var rank = (long) (rankCommand.ExecuteScalar() ?? throw new Exception("Could not get user rank!")) + 1;

        if (!reader.IsDBNull(3))
            badge = GetBadge(reader.GetString(3));
            
        if (!reader.IsDBNull(4))
            discordId = reader.GetString(4);

        var banned = reader.GetBoolean(5);
            
        return new UserInfo(userName, userId, mmr, badge, rank, discordId, banned);
    }

    public List<UserInfo> GetAllUsers()
    {
        var command = Connection.CreateCommand();
        command.CommandText = "SELECT * FROM userData ORDER BY mmr DESC";
        
        var userList = new List<UserInfo>();
        
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            var user = GetUserInfoFromReader(reader);
            if (user == null) continue;
            userList.Add(user);
        }
        
        return userList;
    }

    public void ApplyMmrChange(UserInfo user, int mmrChange)
    {
        var command = Connection.CreateCommand();
        command.CommandText = "UPDATE userData SET mmr = @newMmr WHERE userData.id = @id";
        command.Parameters.AddWithValue("newMmr", Math.Max(0, user.Mmr + mmrChange));
        command.Parameters.AddWithValue("id", user.UserId);
        command.ExecuteNonQuery();
    }

    private Badge? GetBadge(string? badgeName)
    {
        if (badgeName == null) return null;
        
        var command = Connection.CreateCommand();
        command.CommandText = "SELECT * FROM badges WHERE badgeName = @badgeName LIMIT 1";
        command.Parameters.AddWithValue("@badgeName", badgeName);
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            if (reader.FieldCount == 0) return null;
            
            var name = reader.GetString(0);
            var color = reader.GetString(1);
            var bold = reader.GetBoolean(2);
            
            return new Badge(name, color, bold);
        }
        
        return null;
    }

    public UserInfo[]? GetAroundUser(string userId)
    {
        return [];
        //TODO: FIX
        // var users = GetAllUsers();
        //
        // if (users.All(i => i?.UserId != userId))
        //     return null;
        //
        // var userIdx = users.FindIndex(i => i?.UserId == userId);
        //
        // return users.Slice(Math.Clamp(userIdx - 4, 0, users.Count), Math.Max(users.Count, users.Count)).ToArray();
    }
    
    public UserInfo[] GetLeaderboardRange(int start, int range)
    {
        range = Math.Min(range, 10);

        var users = GetAllUsers().Where(i => i.Rank >= start).ToArray();
        Array.Resize(ref users, Math.Min(users.Length, range));

        return users;
    }
    
    public UserInfo UpdateUserDataOnLogin(string userId, string userName)
    {
        var user = GetUserById(userId);
        if (user != null)
        {
            var updateUserNameCommand = Connection.CreateCommand();
            updateUserNameCommand.CommandText = "UPDATE userData SET username = @userName WHERE userData.id = @userId";
            updateUserNameCommand.Parameters.AddWithValue("userName", userName);
            updateUserNameCommand.Parameters.AddWithValue("userId", userId);
            updateUserNameCommand.ExecuteNonQuery();
            
            // avoid fetching from db twice in a row
            return GetUserById(userId) ??  throw new Exception("Could not find updated user!");
        }
        
        var addToUserDataCommand = Connection.CreateCommand();
        addToUserDataCommand.CommandText = "INSERT INTO userData VALUES (@userId, 1000, @userName, null, null, false)";
        addToUserDataCommand.Parameters.AddWithValue("userId", userId);
        addToUserDataCommand.Parameters.AddWithValue("userName", userName);
        addToUserDataCommand.ExecuteNonQuery();

        return GetUserById(userId) ?? throw new Exception("Could not find updated user!");
    }
    
    protected override void CreateInitialTables()
    {
        CreateUserDataTable();
        CreateBadgeTable();
    }

    private void CreateBadgeTable()
    {
        var command = Connection.CreateCommand();
        command.CommandText = "CREATE TABLE IF NOT EXISTS badges (badgeName TEXT NOT NULL PRIMARY KEY, badgeColor TEXT NOT NULL, bold BOOLEAN NOT NULL)";
        command.ExecuteNonQuery();
    }
    
    private void CreateUserDataTable()
    {
        var dbCommand = Connection.CreateCommand();
        dbCommand.CommandText = "CREATE TABLE IF NOT EXISTS userData (id TEXT NOT NULL PRIMARY KEY, mmr INTEGER NOT NULL, username TEXT NOT NULL, badge TEXT, discordID TEXT UNIQUE, banned BOOLEAN NOT NULL)";
        dbCommand.ExecuteNonQuery();
    }
}