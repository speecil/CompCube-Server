using LoungeSaber_Server.Models.Badge;
using LoungeSaber_Server.Models.Client;

namespace LoungeSaber_Server.SQL;

public class UserData : Database
{
    public static readonly UserData Instance = new();

    protected override string DatabaseName => "LoungeData";

    public UserInfo? GetUser(string userId)
    {
        var command = _connection.CreateCommand();
        command.CommandText = $"SELECT * FROM userData WHERE userData.id = @id LIMIT 1";
        command.Parameters.AddWithValue("id", userId);
        using var reader = command.ExecuteReader();
        
        while (reader.Read())
        {
            if (reader.FieldCount == 0) 
                return null;
            
            var mmr = reader.GetInt32(1);
            var userName = reader.GetString(2);
            Badge? badge = null;

            if (!reader.IsDBNull(3))
                badge = GetBadge(reader.GetString(3));
            
            return new UserInfo(userName, userId, mmr, badge);
        }

        return null;
    }

    public UserInfo ApplyMmrChange(UserInfo user, int mmrChange)
    {
        var command = _connection.CreateCommand();
        command.CommandText = "UPDATE userData SET mmr = @newMmr WHERE userData.id = @id";
        command.Parameters.AddWithValue("newMmr", user.Mmr + mmrChange);
        command.Parameters.AddWithValue("id", user.UserId);
        command.ExecuteNonQuery();
        
        return GetUser(user.UserId) ?? throw new Exception("Could not find updated user!");
    }

    public Badge? GetBadge(string? badgeName)
    {
        if (badgeName == null) return null;
        
        var command = _connection.CreateCommand();
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
    
    public UserInfo UpdateUserLoginData(string userId, string userName)
    {
        var user = GetUser(userId);
        if (user != null)
        {
            var updateUserNameCommand = _connection.CreateCommand();
            updateUserNameCommand.CommandText = "UPDATE userData SET username = @userName WHERE userData.id = @userId";
            updateUserNameCommand.Parameters.AddWithValue("userName", userName);
            updateUserNameCommand.Parameters.AddWithValue("userId", userId);
            updateUserNameCommand.ExecuteNonQuery();
            
            // avoid fetching from db twice in a row
            return new UserInfo(userName, userId, user.Mmr, user.Badge);
        }
        
        var addToUserDataCommand = _connection.CreateCommand();
        addToUserDataCommand.CommandText = "INSERT INTO userData VALUES (@userId, 1000, @userName, null)";
        addToUserDataCommand.Parameters.AddWithValue("userId", userId);
        addToUserDataCommand.Parameters.AddWithValue("userName", userName);
        addToUserDataCommand.ExecuteNonQuery();

        return new UserInfo(userName, userId, 1000, null);
    }
    
    protected override void CreateInitialTables()
    {
        CreateUserDataTable();
        CreateLinkedDiscordTable();
        CreateBadgeTable();
    }

    private void CreateLinkedDiscordTable()
    {
        var command = _connection.CreateCommand();
        command.CommandText = "CREATE TABLE IF NOT EXISTS discord (id TEXT NOT NULL PRIMARY KEY, discordId TEXT NOT NULL)";
        command.ExecuteNonQuery();
    }

    private void CreateBadgeTable()
    {
        var command = _connection.CreateCommand();
        command.CommandText = "CREATE TABLE IF NOT EXISTS badges (badgeName TEXT NOT NULL PRIMARY KEY, badgeColor TEXT NOT NULL, bold BOOLEAN NOT NULL)";
        command.ExecuteNonQuery();
    }
    
    private void CreateUserDataTable()
    {
        var dbCommand = _connection.CreateCommand();
        dbCommand.CommandText = "CREATE TABLE IF NOT EXISTS userData (id TEXT NOT NULL PRIMARY KEY, mmr INTEGER NOT NULL, username TEXT NOT NULL, badge TEXT)";
        dbCommand.ExecuteNonQuery();
    }
}