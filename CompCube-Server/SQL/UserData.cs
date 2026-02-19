using System.Data.SQLite;
using CompCube_Models.Models.ClientData;
using CompCube_Models.Models.Match;
using CompCube_Server.Divisions;

namespace CompCube_Server.SQL;

public class UserData(RankingData rankingData) : TableManager
{
    public UserInfo? GetUserByDiscordId(string discordId)
    {
        var command = Connection.CreateCommand();
        command.CommandText = "SELECT * FROM userData JOIN rankingData USING (id) WHERE discordId = @discordId AND season = @season LIMIT 1";
        command.Parameters.AddWithValue("discordId", discordId);
        command.Parameters.AddWithValue("season", rankingData.CurrentSeason);
        
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

    public UserInfo? GetUserById(string userId, int season = -1)
    {
        if (season == -1)
            season = rankingData.CurrentSeason;
        
        var command = Connection.CreateCommand();
        command.CommandText = "SELECT * FROM userData JOIN rankingData USING (id) WHERE userData.id = @id AND rankingData.season = @season LIMIT 1";
        command.Parameters.AddWithValue("id", userId);
        command.Parameters.AddWithValue("season", season);
        using var reader = command.ExecuteReader();

        while (reader.Read())
            return GetUserInfoFromReader(reader);
        
        return null;
    }

    public List<UserInfo> GetAllUsers()
    {
        var command = Connection.CreateCommand();
        command.CommandText = "SELECT * FROM userData JOIN rankingData USING (id) WHERE season = @season ORDER BY mmr DESC";
        command.Parameters.AddWithValue("season", rankingData.CurrentSeason);
        
        var userList = new List<UserInfo>();
        
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            var user = GetUserInfoFromReader(reader);
            if (user == null || user.Banned) 
                continue;
            userList.Add(user);
        }
        
        return userList;
    }
    
    private UserInfo? GetUserInfoFromReader(SQLiteDataReader reader)
    {
        var id = reader.GetString(0);
        var userName = reader.GetString(1);
        Badge? badge = null;
        
        if (!reader.IsDBNull(2))
            badge = GetBadge(reader.GetString(2));
        
        string? discordId = null;
        
        if (!reader.IsDBNull(3))
            discordId = reader.GetString(3);
        
        var banned = reader.GetBoolean(4);
        var mmr = reader.GetInt32(6);
        var wins = reader.GetInt32(7);
        var totalGames = reader.GetInt32(8);
        var winstreak = reader.GetInt32(9);
        var bestWinstreak = reader.GetInt32(10);

        using var rankCommand = Connection.CreateCommand();
        rankCommand.CommandText = "SELECT COUNT(*) FROM userData JOIN rankingData USING (id) WHERE mmr > @mmrThreshold AND banned = false AND season = @season ORDER BY mmr";
        rankCommand.Parameters.AddWithValue("@season", rankingData.CurrentSeason);
        rankCommand.Parameters.AddWithValue("@mmrThreshold", mmr);
        var rank = (long) (rankCommand.ExecuteScalar() ?? -1) + 1;

        return new UserInfo(userName, id, mmr, DivisionManager.GetDivisionFromMmr(mmr), badge, rank, discordId, banned, wins, totalGames, winstreak, bestWinstreak);
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
        rankingData.CreateRankingDataForUserIfNotExists(userId);
        
        using var addToUserDataCommand = Connection.CreateCommand();
        addToUserDataCommand.CommandText = "INSERT OR IGNORE INTO userData VALUES (@userId, @userName, null, null, false)";
        addToUserDataCommand.Parameters.AddWithValue("@userId", userId);
        addToUserDataCommand.Parameters.AddWithValue("@userName", userName);
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
        dbCommand.CommandText = "CREATE TABLE IF NOT EXISTS userData (" +
                                "id TEXT NOT NULL PRIMARY KEY, " +
                                "username TEXT NOT NULL, " +
                                "badge TEXT, " +
                                "discordID TEXT UNIQUE, " +
                                "banned BOOLEAN NOT NULL)";
        dbCommand.ExecuteNonQuery();
    }
}