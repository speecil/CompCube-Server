using CompCube_Models.Models.Match;
using CompCube_Server.Gameplay.Match;
using CompCube_Server.Gameplay.Matchmaking;
using CompCube_Server.Logging;
using NetCord;
using NetCord.Gateway;
using NetCord.Rest;

namespace CompCube_Server.Discord.Events;

public class MatchMessageManager
{
    private readonly TextChannel? _matchChannel;
    private readonly MatchInfoMessageFormatter _messageFormatter;
    private readonly Logger _logger;

    public MatchMessageManager(MatchInfoMessageFormatter messageFormatter, Logger logger, GatewayClient gatewayClient, IConfiguration config)
    {
        _messageFormatter = messageFormatter;
        _logger = logger;
        
        var channelId = config.GetSection("Discord").GetValue<ulong>("MatchLoggingChannelId", 0);

        if (channelId == 0)
        {
            _matchChannel = null;
            return;
        }
        
        _matchChannel = gatewayClient.Rest.GetChannelAsync(channelId).Result as TextChannel;
    }

    public async void PostMatchResults(MatchResultsData results)
    {
        try
        {
            if (results.Premature)
                return;

            if (_matchChannel == null) 
                return;

            var embed = _messageFormatter.GetEmbed(results, "Match results:", false);

            await _matchChannel.SendMessageAsync(new MessageProperties()
            {
                Embeds = [embed]
            });
        }
        catch (Exception e)
        {
            _logger.Error(e);
        }
    }
}