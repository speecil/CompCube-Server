using LoungeSaber_Server.Models.Map;

namespace LoungeSaber_Server.SQL;

public class MapData : Database
{
    public static readonly MapData Instance = new();
    
    protected override string DatabaseName => "MapData";
    
    protected override void CreateInitialTables()
    {
        var createDbCommand = _connection.CreateCommand();
        createDbCommand.CommandText = "CREATE TABLE IF NOT EXISTS mapData ( " + 
                                      "hash TEXT NOT NULL, " + 
                                      "difficulty TEXT NOT NULL, " + 
                                      "category TEXT NOT NULL " +
                                      ");";
        createDbCommand.ExecuteNonQuery();
    }

    public List<VotingMap> GetAllMaps()
    {
        var maps = new List<VotingMap>();
        
        var dbCommand = _connection.CreateCommand();
        dbCommand.CommandText = "SELECT * FROM mapData";
        using var reader = dbCommand.ExecuteReader();

        while (reader.Read())
        {
            if (reader.FieldCount == 0) return [];
            
            var hash = reader.GetString(0);

            if (!Enum.TryParse<VotingMap.DifficultyType>(reader.GetString(1), out var difficulty))
            {
                Console.WriteLine($"Could not parse difficulty type for hash {hash}: {reader.GetString(1)}");
                continue;
            }

            if (!Enum.TryParse<VotingMap.CategoryType>(reader.GetString(2), out var category))
            {
                Console.WriteLine($"Could not parse category for hash {hash}: {reader.GetString(2)}");
                continue;
            }
            
            maps.Add(new VotingMap(hash, difficulty, category));
        }
        
        return maps;
    }
}