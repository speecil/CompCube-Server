using System.Globalization;
using CompCube_Models.Models.Match;
using CompCube_Server.Api.BeatSaver;
using NetCord;
using NetCord.Rest;

namespace CompCube_Server.Discord;

public class MatchInfoMessageFormatter(BeatSaverApiWrapper beatSaver)
{
    public async Task<EmbedProperties> GetEmbed(MatchResultsData results, string header, bool showRelativeTimestamp)
    {
        var beatSaverMap = results.Map == null ? null : await beatSaver.GetBeatmapFromHash(results.Map.Hash);

        var embed = new EmbedProperties
        {
            Title = $"{header}",
            Description = beatSaverMap is null ? "" : $"{beatSaverMap?.Metadata.SongAuthorName} - {beatSaverMap?.Metadata.SongName} ({results.Map?.Difficulty}) (https://beatsaver.com/maps/{beatSaverMap?.ID})",
            Thumbnail = new EmbedThumbnailProperties(beatSaverMap?.LatestVersion.CoverURL),
            Fields =
            [
                new()
                {
                    Name = "Winner",
                    Value = $"{FormatMatchScore(results.Winner)} ({results.Winner.User.Mmr} -> {results.Winner.User.Mmr + results.MmrChange})",
                    Inline = true
                },
                new()
                {
                    Name = "Loser",
                    Value = $"{FormatMatchScore(results.Loser)} ({results.Loser.User.Mmr} -> {results.Loser.User.Mmr - results.MmrChange})",
                    Inline = false
                },
                new()
                {
                    Name = "MMR Exchange",
                    Value = results.MmrChange.ToString(),
                    Inline = true
                },
                new()
                {
                    Name = "Match ID",
                    Value = results.Id.ToString(),
                    Inline = true
                },
                new()
                {
                    Name = "Time",
                    Value = $"<t:{new DateTimeOffset(results.Time.ToUniversalTime()).ToUnixTimeSeconds()}:f> {(showRelativeTimestamp ? $"(<t:{new DateTimeOffset(results.Time.ToUniversalTime()).ToUnixTimeSeconds()}:R>)" : "")}",
                    Inline = true
                }
            ]
        };

        return embed;
    }
    
    private static string FormatMatchScore(MatchScore score) =>
        $"{score.User.Username} - {((score.Score ?? Score.Empty).RelativeScore * 100).ToString("F2", CultureInfo.InvariantCulture)}% {(score.Score is { FullCombo: true } ? "FC" : $"{score.Score?.Misses}x")}";
}