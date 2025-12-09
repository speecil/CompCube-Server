using System.Globalization;
using CompCube_Models.Models.Match;
using Newtonsoft.Json;

namespace CompCube_Server.SQL;

public class MatchLog(UserData userData) : Database
{
    private readonly Random _random = new();
    
    protected override void CreateInitialTables()
    {
        var command = Connection.CreateCommand();
        command.CommandText = "CREATE TABLE IF NOT EXISTS matchLog (id INT NOT NULL PRIMARY KEY, winnerIds TEXT NOT NULL, loserIds TEXT NOT NULL, mmrExchange INT NOT NULL, prematureEnd BOOL NOT NULL, time TEXT NOT NULL)";
        command.ExecuteNonQuery();
    }

    public MatchResultsData? GetMatch(int id)
    {
        var command = Connection.CreateCommand();
        command.CommandText = "SELECT * FROM matchLog WHERE id = @id LIMIT 1";
        command.Parameters.AddWithValue("id", id);

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            if (reader.FieldCount == 0)
                return null;
            
            var matchId = reader.GetInt32(0);
            var winners = JsonConvert.DeserializeObject<string[]>(reader.GetString(1))?.Select(i => userData.GetUserById(i) ?? throw new Exception($"Could not find user {i}")).ToArray() ?? [];
            var losers = JsonConvert.DeserializeObject<string[]>(reader.GetString(2))?.Select(i => userData.GetUserById(i) ?? throw new Exception($"Could not find user {i}")).ToArray() ?? [];
            var exchange = reader.GetInt32(3);
            var prematureEnd = reader.GetBoolean(4);
            var time = DateTime.Parse(reader.GetString(5)).ToLocalTime();

            return new MatchResultsData(winners, losers,
                exchange, prematureEnd, matchId, time);
        }

        return null;
    }

    public void AddMatchToTable(MatchResultsData results)
    {
        if (IsMatchIdUsed(results.Id))
            throw new Exception("Match id is already taken!");
        
        var command = Connection.CreateCommand();
        command.CommandText = "INSERT INTO matchLog VALUES (@id, @winnerIds, @loserIds, @mmrExchange, @prematureEnd, @time)";
        
        command.Parameters.AddWithValue("id", results.Id);
        command.Parameters.AddWithValue("winnerIds", JsonConvert.SerializeObject(results.Winner.Select(i => i.UserId)));
        command.Parameters.AddWithValue("loserIds", JsonConvert.SerializeObject(results.Loser.Select(i => i.UserId)));
        command.Parameters.AddWithValue("mmrExchange", results.MmrChange);
        command.Parameters.AddWithValue("prematureEnd", results.Premature);
        command.Parameters.AddWithValue("time", results.Time.ToString(CultureInfo.InvariantCulture));
        
        results.Winner.ToList().ForEach(i => userData.UpdateUserDataFromMatch(i, results.MmrChange, results.Winner.Contains(i)));

        command.ExecuteNonQuery();
    }

    public int GetValidMatchId()
    {
        var idArr = new int[6];

        for (var i = 0; i < idArr.Length; i++)
            idArr[i] = _random.Next(0, 10);

        var id = int.Parse(string.Join("", idArr));

        if (IsMatchIdUsed(id)) 
            return GetValidMatchId();

        return id;
    }

    private bool IsMatchIdUsed(int matchId)
    {
        var match = GetMatch(matchId);

        return match != null;
    }
}