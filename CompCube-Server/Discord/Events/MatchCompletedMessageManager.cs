using System.Globalization;
using CompCube_Models.Models.Match;
using CompCube_Server.Gameplay.Match;
using CompCube_Server.Gameplay.Matchmaking;
using CompCube_Server.Logging;
using CompCube_Server.Interfaces;
using NetCord;
using NetCord.Rest;

namespace CompCube_Server.Discord.Events;

public class MatchCompletedMessageManager(QueueManager queueManager, MatchInfoMessageFormatter messageFormatter, Logger logger)
{
    private TextChannel? _channel;

    public void Init(TextChannel channel)
    {
        if (_channel != null) 
            return;
        
        _channel = channel;

        queueManager.OnAnyMatchEnded += OnMatchEnded;
    }

    public void Stop()
    {
        queueManager.OnAnyMatchEnded -= OnMatchEnded;
    }

    private async void OnMatchEnded(MatchResultsData results, Match match)
    {
        try
        {
            if (results.Premature || results.Map == null)
                return;

            if (_channel == null) 
                return;

            var embed = await messageFormatter.GetEmbed(results, "Match results:", false);

            await _channel.SendMessageAsync(new MessageProperties()
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