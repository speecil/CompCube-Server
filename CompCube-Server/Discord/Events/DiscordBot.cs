using CompCube_Models.Models.ClientData;
using CompCube_Models.Models.Match;
using CompCube_Server.Gameplay.Match;
using CompCube_Server.Gameplay.Matchmaking;
using CompCube_Server.Interfaces;
using CompCube_Server.Logging;
using NetCord;
using NetCord.Gateway;
using NetCord.Rest;

namespace CompCube_Server.Discord.Events;

public class DiscordBot : IDiscordBot
{
    private readonly EventMessageManager _eventMessageManager;
    private readonly MatchMessageManager _matchMessageManager;

    public DiscordBot(MatchInfoMessageFormatter messageFormatter, Logger logger, GatewayClient gatewayClient, IConfiguration config)
    {
        var discordConfigSection = config.GetSection("Discord");
        
        var matchChannelId = discordConfigSection.GetValue<ulong>("MatchLoggingChannelId", 0);

        TextChannel? matchChannel = null;
        
        if (matchChannelId != 0)
        {
            matchChannel = gatewayClient.Rest.GetChannelAsync(matchChannelId).Result as TextChannel;
        }
        
        TextChannel? eventsChannel = null;
        
        var eventsChannelId = discordConfigSection.GetValue<ulong>("EventsLoggingChannelId", 0);

        if (eventsChannelId != 0)
        {
            eventsChannel = gatewayClient.Rest.GetChannelAsync(eventsChannelId).Result as TextChannel;
        }
        
        _eventMessageManager = new EventMessageManager(eventsChannel);
        _matchMessageManager = new MatchMessageManager(matchChannel, logger, messageFormatter);
    }

    public void PostMatchResults(MatchResultsData matchResults) => _matchMessageManager.PostMatchResults(matchResults);

    public void PostEventScores(List<MatchScore> scores, List<UserInfo> usersWithoutScores) => _eventMessageManager.PostEventScores(scores, usersWithoutScores);

    public void PostEventPoints(Dictionary<UserInfo, int> points) => _eventMessageManager.PostEventPoints(points);
}