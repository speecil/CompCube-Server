using System.Data.SQLite;
using CompCube_Models.Models.ClientData;
using CompCube_Models.Models.Match;
using CompCube_Server.Divisions;

namespace CompCube_Server.SQL;

public class UserData : TableManager
{
    public UserInfo? GetUserByDiscordId(string discordId)
    {
        var command = Connection.CreateCommand();
        command.CommandText = "SELECT * FROM userData JOIN rankingData USING (id) WHERE discordId = @discordId LIMIT 1";
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
        command.CommandText = "SELECT * FROM userData JOIN rankingData USING (id) WHERE userData.id = @id LIMIT 1";
        command.Parameters.AddWithValue("id", userId);
        using var reader = command.ExecuteReader();

        while (reader.Read())
            return GetUserInfoFromReader(reader);
        
        return null;
    }

    public void UpdateUserDataFromMatch(UserInfo userInfo, int mmrChange, bool won)
    {
        SetMmr(userInfo, won ? mmrChange : -mmrChange);
        
        var command = Connection.CreateCommand();
        command.CommandText = "UPDATE rankingData SET totalGames = @newTotalGames WHERE id = @id";
        command.Parameters.AddWithValue("newTotalGames", userInfo.TotalGames + 1);
        command.Parameters.AddWithValue("id", userInfo.UserId);
        command.ExecuteNonQuery();

        if (!won)
        {
            command.CommandText = "UPDATE rankingData SET bestWinstreak = 0 WHERE id = @id LIMIT 1";
            command.Parameters.AddWithValue("id", userInfo.UserId);
            command.ExecuteNonQuery();
            return;
        }
        
        command.CommandText = "UPDATE rankingData SET wins = @newWins WHERE id = @id LIMIT 1";
        command.Parameters.AddWithValue("newWins", userInfo.Wins + 1);
        command.Parameters.AddWithValue("id", userInfo.UserId);
        command.ExecuteNonQuery();

        command = Connection.CreateCommand();
        command.CommandText = "UPDATE rankingData SET winstreak = @newWinstreak WHERE id = @id LIMIT 1";
        command.Parameters.AddWithValue("newWinstreak", userInfo.Winstreak + 1);
        command.Parameters.AddWithValue("id", userInfo.UserId);
        command.ExecuteNonQuery();

        if (userInfo.Winstreak + 1 <= userInfo.HighestWinstreak) 
            return;
            
        command = Connection.CreateCommand();
        command.CommandText = "UPDATE rankingData SET bestWinstreak = @newHighestWinstreak WHERE id = @id LIMIT 1";
        command.Parameters.AddWithValue("newHighestWinstreak", userInfo.HighestWinstreak + 1);
        command.Parameters.AddWithValue("id", userInfo.UserId);
        command.ExecuteNonQuery();
    }

    public List<UserInfo> GetAllUsers()
    {
        var command = Connection.CreateCommand();
        command.CommandText = "SELECT * FROM userData JOIN rankingData USING (id) ORDER BY mmr DESC";
        
        var userList = new List<UserInfo>();
        
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            var user = GetUserInfoFromReader(reader);
            if (user == null) 
                continue;
            userList.Add(user);
        }
        
        return userList;
    }
    
    private UserInfo? GetUserInfoFromReader(SQLiteDataReader reader)
    {
        if (reader.FieldCount == 0) 
            return null;
        
        var userId =  reader.GetString(0);    
        var mmr = reader.GetInt32(5);
        var userName = reader.GetString(1);
        Badge? badge = null;
        string? discordId = null;

        var rankCommand = Connection.CreateCommand();
        rankCommand.CommandText = "SELECT COUNT(*) FROM rankingData WHERE mmr > @mmrThreshold ORDER BY mmr";
        rankCommand.Parameters.AddWithValue("mmrThreshold", mmr);
        var rank = (long) (rankCommand.ExecuteScalar() ?? throw new Exception("Could not get user rank!")) + 1;

        if (!reader.IsDBNull(2))
            badge = GetBadge(reader.GetString(2));
            
        if (!reader.IsDBNull(3))
            discordId = reader.GetString(3);

        var banned = reader.GetBoolean(4);

        var wins = reader.GetInt32(6);
        var totalGames = reader.GetInt32(7);
        var winstreak = reader.GetInt32(8);
        var highestWinstreak = reader.GetInt32(9);
            
        return new UserInfo(userName, userId, mmr, DivisionManager.GetDivisionFromMmr(mmr), badge, rank, discordId, banned, wins, totalGames, winstreak, highestWinstreak);
    }

    public void SetMmr(UserInfo user, int newMmr)
    {
        var command = Connection.CreateCommand();
        command.CommandText = "UPDATE rankingData SET mmr = @newMmr WHERE rankingData.id = @id";
        command.Parameters.AddWithValue("newMmr", Math.Max(0, newMmr));
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

    // fixed. i think
    public UserInfo[]? GetAroundUser(string userId)
    {
        var users = GetAllUsers().Where(i => !i.Banned).ToArray();

        if (users.Length == 0 || users.All(u => u.UserId != userId))
            return null;

        var index = Array.FindIndex(users, u => u.UserId == userId);
        if (index < 0)
            return null;

        var startIndex = Math.Max(0, index - 5);
        var count = Math.Min(10, users.Length - startIndex);

        return users.Skip(startIndex).Take(count).ToArray();
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
        var addToUserDataCommand = Connection.CreateCommand();
        addToUserDataCommand.CommandText = "INSERT OR IGNORE INTO userData VALUES (@userId, @userName, null, null, false)";
        addToUserDataCommand.Parameters.AddWithValue("@userId", userId);
        addToUserDataCommand.Parameters.AddWithValue("@userName", userName);
        addToUserDataCommand.ExecuteNonQuery();
        
        var addRankingDataCommand = Connection.CreateCommand();
        addRankingDataCommand.CommandText = "INSERT OR IGNORE INTO rankingData VALUES (@userId, 1000, 0, 0, 0, 0)";
        addRankingDataCommand.Parameters.AddWithValue("@userId", userId);
        addRankingDataCommand.ExecuteNonQuery();

        return GetUserById(userId) ?? throw new Exception("Could not find updated user!");
    }
    
    protected override void CreateInitialTables()
    {
        CreateUserDataTable();
        CreateRankingDataTable();
        CreateBadgeTable();
    }

    private void CreateBadgeTable()
    {
        var command = Connection.CreateCommand();
        command.CommandText = "CREATE TABLE IF NOT EXISTS badges (badgeName TEXT NOT NULL PRIMARY KEY, badgeColor TEXT NOT NULL, bold BOOLEAN NOT NULL)";
        command.ExecuteNonQuery();
    }

    private void CreateRankingDataTable()
    {
        var command = Connection.CreateCommand();

        command.CommandText = "CREATE TABLE IF NOT EXISTS rankingData (id TEXT NOT NULL PRIMARY KEY, mmr INT NOT NULL, wins INT NOT NULL DEFAULT 0, totalGames INT NOT NULL DEFAULT 0, winstreak INT NOT NULL DEFAULT 0, bestWinstreak INT NOT NULL DEFAULT 0)";
        command.ExecuteNonQuery();
    }
    
    private void CreateUserDataTable()
    {
        var dbCommand = Connection.CreateCommand();
        dbCommand.CommandText = "CREATE TABLE IF NOT EXISTS userData (" +
                                "id TEXT NOT NULL PRIMARY KEY, " +
                                "username TEXT NOT NULL, " +
                                "badge TEXT, " +
                                "discordID TEXT UNIQUE, " +
                                "banned BOOLEAN NOT NULL)";
        dbCommand.ExecuteNonQuery();
    }
}