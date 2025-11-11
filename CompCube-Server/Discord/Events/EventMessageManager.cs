using CompCube_Models.Models.ClientData;
using CompCube_Models.Models.Match;
using NetCord;
using NetCord.Gateway;
using NetCord.Rest;

namespace CompCube_Server.Discord.Events;

public class EventMessageManager(GatewayClient gatewayClient)
{
    private readonly TextChannel? _eventsChannel = gatewayClient.Rest.GetChannelAsync(1400279911008174282).Result as TextChannel;

    public void PostEventScores(List<MatchScore> scores, List<UserInfo> usersWithoutScores)
    {
        MessageProperties messageProperties = "";

        messageProperties.Embeds =
        [
            new EmbedProperties()
            {
                Title = "Scores:",
                Description = FormatStringsIntoPositions(scores.Select(i => $"{i.User.Username} - {(i.Score?.RelativeScore * 100):F2}").Concat(usersWithoutScores.Select(i => $"{i.Username} - N/A")).ToArray())
            }
        ];

        _eventsChannel?.SendMessageAsync(messageProperties);
    }

    public void PostEventPoints(Dictionary<UserInfo, int> points)
    {
        MessageProperties message = "";

        message.Embeds =
        [
            new EmbedProperties()
            {
                Title = "Current Standings:",
                Description = FormatStringsIntoPositions(points.Select(i => $"{i.Key.Username} - {i.Value}").ToArray())
            }
        ];
        
        _eventsChannel?.SendMessageAsync(message);
    }

    private string FormatStringsIntoPositions(string[] strings)
    {
        var returnStr = "";

        for (var i = 0; i < strings.Length; i++)
        {
            returnStr += $"{i + 1}. {strings[i]}\n";
        }
        
        return returnStr;
    }
}