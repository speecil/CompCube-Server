using CompCube_Models.Models.ClientData;
using CompCube_Server.SQL;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using RomanNumerals;

namespace CompCube_Server.Discord.Commands;

public class LeaderboardCommands(UserData userData) : ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand("leaderboard", "leaderboard")]
    public InteractionMessageProperties Leaderboard(int page = 1)
    {
        page -= 1;

        var leaderboard = userData.GetLeaderboardRange(page, page + 10);

        return new InteractionMessageProperties
        {
            Embeds = [GetLeaderboardEmbedFromUsers(leaderboard)]
        };
    }

    [SlashCommand("nearme", "get leaderboard data around you")]
    public InteractionMessageProperties NearMe()
    {
        var userInfo = userData.GetUserByDiscordId(Context.User.Id.ToString());

        if (userInfo == null)
            return "Could not get user info! (Did you link your discord account?)";

        var users = userData.GetAroundUser(userInfo.UserId);

        if (users == null)
            return "Could not find players around you!";

        return new InteractionMessageProperties
        {
            Embeds = [GetLeaderboardEmbedFromUsers(users)]
        };
    }

    private EmbedProperties GetLeaderboardEmbedFromUsers(UserInfo[] userList, int pageNumber = -1) =>
        new()
        {
            Title = pageNumber == -1 ? "Leaderboard" : $"Leaderboard (Page {pageNumber})",
            Description = userList.Select(user => $"\n{user.Rank}. {user.Username} - {user.Mmr:N0} MMR ({user.Division.Division} {new RomanNumeral(user.Division.SubDivision)})").Aggregate("", (current, lineString) => current + lineString)
        };

}