using LoungeSaber_Server.Models;
using LoungeSaber_Server.Models.Maps;
using Microsoft.Data.Sqlite;

namespace LoungeSaber_Server.SQL;

public class MapData : Database
{
    public static readonly MapData Instance = new();
    
    protected override string DatabaseName => "MapData";
    
    protected override void CreateInitialTable()
    {
        var createDBCommand = _connection.CreateCommand();
        createDBCommand.CommandText = "CREATE TABLE IF NOT EXISTS mapData ( " + 
                                      "hash TEXT NOT NULL, " + 
                                      "difficulty TEXT NOT NULL, " + 
                                      "characteristic TEXT NOT NULL, " + 
                                      "category TEXT NOT NULL " +
                                      ");";
        createDBCommand.ExecuteNonQuery();
    }

    public List<VotingMap> GetAllIndividualDifficulties()
    {
        var command = _connection.CreateCommand();
        command.CommandText = "SELECT * FROM mapData;";

        var maps = new List<VotingMap>();

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            var hash = reader.GetString(0);
            var difficulty = reader.GetString(1);
            var characteristic = reader.GetString(2);

            if (!Enum.TryParse<MapDifficulty.MapCategory>(reader.GetString(3), out var category))
            {
                Console.WriteLine($"Couldn't get category data for hash {hash}!");
                category = MapDifficulty.MapCategory.Unknown;
            }
            
            maps.Add(new VotingMap(hash, new MapDifficulty(characteristic, difficulty, category)));
        }

        return maps;
    }

    public List<Map> GetAllMaps()
    {
        var votingMaps = GetAllIndividualDifficulties();

        var completeMaps = new List<Map>();

        foreach (var map in votingMaps)
        {
            var alreadyExistingMap = completeMaps.FirstOrDefault(i => i.Hash == map.Hash);

            if (alreadyExistingMap != null)
            {
                alreadyExistingMap.Difficulties.Add(new MapDifficulty(map.Characteristic, map.Difficulty, map.Category));
                continue;
            }
            
            completeMaps.Add(new Map(map.Hash, [new MapDifficulty(map.Characteristic, map.Difficulty, map.Category)]));
        }

        return completeMaps;
    }
}