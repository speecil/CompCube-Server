using CompCube_Models.Models.Match;
using CompCube_Server.Logging;
using NetCord;
using NetCord.Rest;

namespace CompCube_Server.Discord.Events;

public class MatchMessageManager(TextChannel? matchChannel, Logger logger, MatchInfoMessageFormatter messageFormatter)
{
    public async Task PostMatchResults(MatchResultsData results)
    {
        if (results.Premature)
            return;

        if (matchChannel == null)
            return;

        var embed = messageFormatter.GetEmbed(results, "Match results:", false);

        await matchChannel.SendMessageAsync(new MessageProperties()
        {
            Embeds = [embed]
        });
    }
}