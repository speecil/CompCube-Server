using System.Globalization;
using CompCube_Models.Models.Match;
using CompCube_Server.Api.BeatSaver;
using NetCord;
using NetCord.Rest;

namespace CompCube_Server.Discord;

public class MatchInfoMessageFormatter
{
    public EmbedProperties GetEmbed(MatchResultsData results, string header, bool showRelativeTimestamp)
    {
        var embed = new EmbedProperties
        {
            Title = $"{header}",
            Fields =
            [
                /*new()
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
                },*/
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
}