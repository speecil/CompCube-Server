using CompCube_Models.Models.ClientData;
using CompCube_Models.Models.Match;
using CompCube_Server.Logging;

namespace CompCube_Server.SQL;

public class RankingData(IConfiguration config, Logger logger) : TableManager
{
    public int CurrentSeason => config.GetSection("Server").GetValue("Season", 0);
    
    protected override void CreateInitialTables()
    {
        var command = Connection.CreateCommand();

        command.CommandText = "CREATE TABLE IF NOT EXISTS rankingData (season INT NOT NULL, id TEXT NOT NULL, mmr INT NOT NULL, wins INT NOT NULL DEFAULT 0, totalGames INT NOT NULL DEFAULT 0, winstreak INT NOT NULL DEFAULT 0, bestWinstreak INT NOT NULL DEFAULT 0)";
        command.ExecuteNonQuery();
    }
    
    public void UpdateUserDataFromMatch(MatchResultsData results, int transfer, int penaltyForDisconnect)
    {
        AdjustMmr(results.Winner.First().UserId, transfer);
        AdjustMmr(results.Loser.First().UserId, -transfer);
        
        if (results.Premature)
            AdjustMmr(results.Loser.First().UserId, -penaltyForDisconnect);

        IncrementWins(results.Winner.First());
        ResetWinstreak(results.Loser.First());
        
        IncrementTotalGames(results.Winner.First());
        IncrementTotalGames(results.Loser.First());
    }

    private void IncrementWins(UserInfo user)
    {
        using var incrementWinsCommand = Connection.CreateCommand();
        incrementWinsCommand.CommandText = "UPDATE rankingData SET wins = wins + 1 WHERE id = @id AND season = @season LIMIT 1";
        incrementWinsCommand.Parameters.AddWithValue("@id", user.UserId);
        incrementWinsCommand.Parameters.AddWithValue("@season", CurrentSeason);
        incrementWinsCommand.ExecuteNonQuery();

        using var incrementWinstreakCommand = Connection.CreateCommand();
        incrementWinstreakCommand.CommandText = "UPDATE rankingData SET winstreak = winstreak + 1 WHERE id = @id AND season = @season LIMIT 1";
        incrementWinstreakCommand.Parameters.AddWithValue("@id", user.UserId);
        incrementWinstreakCommand.Parameters.AddWithValue("@season", CurrentSeason);
        incrementWinstreakCommand.ExecuteNonQuery();

        if (user.Winstreak + 1 < user.HighestWinstreak)
            return;

        using var incrementBestWinstreakCommand = Connection.CreateCommand();
        incrementBestWinstreakCommand.CommandText = "UPDATE rankingData SET bestWinstreak = winstreak WHERE id = @id AND season = @season LIMIT 1";
        incrementBestWinstreakCommand.Parameters.AddWithValue("@id", user.UserId);
        incrementBestWinstreakCommand.Parameters.AddWithValue("@season", CurrentSeason);
        incrementBestWinstreakCommand.ExecuteNonQuery();
    }

    private void ResetWinstreak(UserInfo user)
    {
        using var resetWinstreakCommand = Connection.CreateCommand();
        resetWinstreakCommand.CommandText = "UPDATE rankingData SET winstreak = 0 WHERE id = @id AND season = @season LIMIT 1";
        resetWinstreakCommand.Parameters.AddWithValue("@id", user.UserId);
        resetWinstreakCommand.Parameters.AddWithValue("@season", CurrentSeason);
        resetWinstreakCommand.ExecuteNonQuery();
    }

    private void IncrementTotalGames(UserInfo user)
    {
        using var incrementTotalGamesCommand = Connection.CreateCommand();
        incrementTotalGamesCommand.CommandText = "UPDATE rankingData SET totalGames = totalGames + 1 WHERE id = @id AND season = @season LIMIT 1";
        incrementTotalGamesCommand.Parameters.AddWithValue("@id", user.UserId);
        incrementTotalGamesCommand.Parameters.AddWithValue("@season", CurrentSeason);
        incrementTotalGamesCommand.ExecuteNonQuery();
    }
    
    private void AdjustMmr(string userId, int newMmr)
    {
        var command = Connection.CreateCommand();
        command.CommandText = "UPDATE rankingData SET mmr = @newMmr WHERE rankingData.id = @id AND season = @season LIMIT 1";
        command.Parameters.AddWithValue("season", CurrentSeason);
        command.Parameters.AddWithValue("newMmr", Math.Max(0, newMmr));
        command.Parameters.AddWithValue("id", userId);
        command.ExecuteNonQuery();
    }
    
    public void CreateRankingDataForUserIfNotExists(string userId)
    {
        using var indexCommand = Connection.CreateCommand();
        
        indexCommand.CommandText = "SELECT COUNT(*) FROM rankingData WHERE id = @userId AND season = @season";
        indexCommand.Parameters.AddWithValue("@userId", userId);
        indexCommand.Parameters.AddWithValue("@season", CurrentSeason);
        var result = (long) indexCommand.ExecuteScalar();

        if (result >= 1)
            return;

        using var command = Connection.CreateCommand();
        command.CommandText = "INSERT INTO rankingData VALUES (@season, @id, 1000, 0, 0, 0, 0)";
        command.Parameters.AddWithValue("@id", userId);
        command.Parameters.AddWithValue("@season", CurrentSeason);
        command.ExecuteNonQuery();
    }
}