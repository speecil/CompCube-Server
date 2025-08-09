using System.Globalization;
using LoungeSaber_Server.BeatSaverApi;
using LoungeSaber_Server.Gameplay.Match;
using LoungeSaber_Server.Gameplay.Matchmaking;
using LoungeSaber_Server.Interfaces;
using LoungeSaber_Server.Models.Match;
using NetCord;
using NetCord.Rest;

namespace LoungeSaber_Server.Discord.Events;

public class MatchCompletedMessageManager(IMatchmaker matchmaker, MatchInfoMessageFormatter messageFormatter)
{
    private TextChannel? _channel;

    public void Init(TextChannel channel)
    {
        if (_channel != null) 
            return;
        
        _channel = channel;
        
        matchmaker.OnMatchStarted += OnMatchStarted;
    }

    public void Stop()
    {
        matchmaker.OnMatchStarted -= OnMatchStarted;
    }
    
    private void OnMatchStarted(Match match) => match.OnMatchEnded += OnMatchEnded;

    private async void OnMatchEnded(MatchResultsData results, Match match)
    {
        try
        {
            match.OnMatchEnded -= OnMatchEnded;

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
            Console.WriteLine(e);
        }
    }
}