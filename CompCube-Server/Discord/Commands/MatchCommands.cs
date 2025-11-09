using CompCube_Server.SQL;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace CompCube_Server.Discord.Commands;

public class MatchCommands(MatchLog matchLog, MatchInfoMessageFormatter messageFormatter) : ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand("matchinfo", "Get info about a match!")]
    public async Task<InteractionMessageProperties> MatchInfo(int matchId)
    {
        var match = matchLog.GetMatch(matchId);

        if (match == null)
            return $"Could not find match with id {matchId}";

        var embed = await messageFormatter.GetEmbed(match, "Match Info:", true);

        return new InteractionMessageProperties()
        {
            Embeds = [embed]
        };
    }
}