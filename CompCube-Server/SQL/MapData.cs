using CompCube_Models.Models.Map;
using CompCube_Server.Logging;

namespace CompCube_Server.SQL;

public class MapData(Logger logger) : Database
{
    protected override string DatabaseName => "MapData";
    
    protected override void CreateInitialTables()
    {
        var createDbCommand = Connection.CreateCommand();
        createDbCommand.CommandText = "CREATE TABLE IF NOT EXISTS mapData ( " + 
                                      "hash TEXT NOT NULL, " + 
                                      "difficulty TEXT NOT NULL, " + 
                                      "category TEXT NOT NULL " +
                                      ");";
        createDbCommand.ExecuteNonQuery();
    }

    public void AddMap(VotingMap votingMap)
    {
        var command = Connection.CreateCommand();
        command.CommandText = "INSERT INTO mapData VALUES (@hash, @difficulty, @category)";
        command.Parameters.AddWithValue("hash", votingMap.Hash);
        command.Parameters.AddWithValue("difficulty", votingMap.Difficulty.ToString());
        command.Parameters.AddWithValue("category", votingMap.Category.ToString());

        command.ExecuteNonQuery();
    }

    public List<VotingMap> GetAllMaps()
    {
        var maps = new List<VotingMap>();
        
        var dbCommand = Connection.CreateCommand();
        dbCommand.CommandText = "SELECT * FROM mapData";
        using var reader = dbCommand.ExecuteReader();

        while (reader.Read())
        {
            if (reader.FieldCount == 0) return [];
            
            var hash = reader.GetString(0);

            if (!Enum.TryParse<VotingMap.DifficultyType>(reader.GetString(1), out var difficulty))
            {
                logger.Error($"Could not parse difficulty type for hash {hash}: {reader.GetString(1)}");
                continue;
            }

            var category = reader.GetString(2);
            
            maps.Add(new VotingMap(hash, difficulty, category));
        }
        
        return maps;
    }
}