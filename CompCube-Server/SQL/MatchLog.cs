using System.Globalization;
using BeatSaverSharp.Models;
using CompCube_Models.Models.Map;
using CompCube_Models.Models.Match;

namespace CompCube_Server.SQL;

public class MatchLog(UserData userData) : Database
{
    private readonly UserData _userData = userData;
    
    private readonly Random _random = new();
    
    protected override string DatabaseName => "MatchLog";
    
    protected override void CreateInitialTables()
    {
        var command = Connection.CreateCommand();
        command.CommandText = "CREATE TABLE IF NOT EXISTS matchLog (id INT NOT NULL PRIMARY KEY, winnerId TEXT NOT NULL, loserId TEXT NOT NULL, mmrExchange INT NOT NULL, playerOneScore TEXT, playerTwoScore TEXT, prematureEnd BOOL NOT NULL, map TEXT, time TEXT NOT NULL)";
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
            var winner = _userData.GetUserById(reader.GetString(1)) ?? throw new Exception($"Could not find user {reader.GetString(1)}");
            var loser = _userData.GetUserById(reader.GetString(2)) ?? throw new Exception($"Could not find user {reader.GetString(2)}");
            var exchange = reader.GetInt32(3);
            var winnerScore = Score.Deserialize(reader.GetString(4));
            var loserScore = Score.Deserialize(reader.GetString(5));
            var prematureEnd = reader.GetBoolean(6);
            var map = VotingMap.Deserialize(reader.GetString(7));
            var time = DateTime.Parse(reader.GetString(8)).ToLocalTime();

            return new MatchResultsData(new MatchScore(winner, winnerScore), new MatchScore(loser, loserScore),
                exchange, map, prematureEnd, matchId, time);
        }

        return null;
    }

    public void AddMatchToTable(MatchResultsData results)
    {
        if (IsMatchIdUsed(results.Id))
            throw new Exception("Match id is already taken!");
        
        var command = Connection.CreateCommand();
        command.CommandText = "INSERT INTO matchLog VALUES (@id, @winnerId, @loserId, @mmrExchange, @winnerScore, @loserScore, @prematureEnd, @map, @time)";
        
        command.Parameters.AddWithValue("id", results.Id);
        command.Parameters.AddWithValue("winnerId", results.Winner.User.UserId);
        command.Parameters.AddWithValue("loserId", results.Loser.User.UserId);
        command.Parameters.AddWithValue("mmrExchange", results.MmrChange);
        command.Parameters.AddWithValue("winnerScore", results.Winner.Score?.Serialize());
        command.Parameters.AddWithValue("loserScore", results.Loser.Score?.Serialize());
        command.Parameters.AddWithValue("prematureEnd", results.Premature);
        command.Parameters.AddWithValue("map", results.Map?.Serialize());
        command.Parameters.AddWithValue("time", results.Time.ToString(CultureInfo.InvariantCulture));

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