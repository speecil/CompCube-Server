using System.Drawing;
using CompCube_Models.Models.ClientData;
using CompCube_Server.SQL;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using Color = NetCord.Color;

namespace CompCube_Server.Discord.Commands;

public class UserCommands(UserData userData) : ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand("link", "Link your discord account to your LoungeSaber profile!")]
    public string Link(string scoresaberId)
    {
        if (userData.GetUserByDiscordId(Context.User.Id.ToString()) != null)
            return "This user is already linked to a discord account.";

        var userInfo = userData.GetUserById(scoresaberId);
        
        if (userInfo == null)
            return "This user does not have a LoungeSaber account yet!";
        
        userData.LinkDiscordToUser(userInfo.UserId, Context.User.Id.ToString());

        return $"Successfully linked discord account {Context.User.Username} to user {userInfo.Username}";
    }

    [SlashCommand("profile", "View the profile of yourself or another user")]
    public InteractionMessageProperties ProfileByUser(User? user = null, string? id = null)
    {
        if (id != null)
        {
            var userById = userData.GetUserById(id);
            return GetUserProfileMessage(userById);
        }
        
        if (user == null)
            user = Context.User;
        
        var userProfile = userData.GetUserByDiscordId(user.Id.ToString());
        
        return GetUserProfileMessage(userProfile);
    }

    private InteractionMessageProperties GetUserProfileMessage(UserInfo? userInfo)
    {
        InteractionMessageProperties message = "";
        var embed = new EmbedProperties();
        message.Embeds = [embed];

        if (userInfo == null)
        {
            embed.Description = "This user is not linked to a LoungeSaber profile.";
            return message;
        }

        embed.Title = userInfo.Username;
        
        if (userInfo.Badge != null)
            embed.Color = ParseColor(userInfo.Badge.ColorCode);
        
        embed.Fields = 
        [
            new()
            {
                Name = "MMR",
                Value = userInfo.Mmr.ToString(),
                Inline = true
            },
            new()
            {
                Name = "Rank",
                Value = userInfo.Rank.ToString(),
                Inline = true
            }
        ];

        return message;
    }

    private Color ParseColor(string colorCode)
    {
        var drawingColor = ColorTranslator.FromHtml(colorCode);
        
        return new Color(drawingColor.R, drawingColor.G, drawingColor.B);
    }
}