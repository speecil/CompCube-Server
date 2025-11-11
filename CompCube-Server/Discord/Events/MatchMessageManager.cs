using CompCube_Models.Models.Match;
using CompCube_Server.Gameplay.Match;
using CompCube_Server.Gameplay.Matchmaking;
using CompCube_Server.Logging;
using NetCord;
using NetCord.Gateway;
using NetCord.Rest;

namespace CompCube_Server.Discord.Events;

public class MatchMessageManager(MatchInfoMessageFormatter messageFormatter, Logger logger, GatewayClient gatewayClient)
{
    private readonly TextChannel? _matchChannel = gatewayClient.Rest.GetChannelAsync(1400279911008174282).Result as TextChannel;
    
    public async void PostMatchResults(MatchResultsData results)
    {
        try
        {
            if (results.Premature || results.Map == null)
                return;

            if (_matchChannel == null) 
                return;

            var embed = await messageFormatter.GetEmbed(results, "Match results:", false);

            await _matchChannel.SendMessageAsync(new MessageProperties()
            {
                Embeds = [embed]
            });
        }
        catch (Exception e)
        {
            logger.Error(e);
        }
    }
}