using System.Data;
using LoungeSaber_Server.Models;
using Microsoft.Data.Sqlite;

namespace LoungeSaber_Server.SQL;

public static class Database
{
    private static readonly SqliteConnection _connection = new($"Data Source={Path.Combine(DataFolderPath, "LoungeData.db")}");

    private static string DataFolderPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");

    public static bool IsOpen => _connection.State == ConnectionState.Open;
    
    public static void Start()
    {
        if (IsOpen) return;

        Directory.CreateDirectory(DataFolderPath);
        
        _connection.Open();
        
        var createDBCommand = _connection.CreateCommand();
        createDBCommand.CommandText = "CREATE TABLE IF NOT EXISTS userData ( id TEXT NOT NULL PRIMARY KEY, mmr INTEGER NOT NULL );";
        createDBCommand.ExecuteNonQuery();
    }

    public static User? GetUser(string ID)
    {
        var command = _connection.CreateCommand();
        command.CommandText = $"SELECT * FROM userData WHERE userData.id = {ID}";
        using var reader = command.ExecuteReader();
        
        while (reader.Read())
        {
            if (reader.FieldCount == 0) 
                return null;
            
            var id = reader.GetString(0);
            var mmr = reader.GetInt32(1);
            return new User(id, mmr);
        }

        return null;
    }

    public static void Stop()
    {
        if (!IsOpen) return;
        _connection.Close();
    }
}