using LoungeSaber_Server.Models.Client;
using LoungeSaber_Server.SQL;
using Microsoft.AspNetCore.Mvc;

namespace LoungeSaber_Server.Api.Controllers;

[ApiController]
public class UserApiController : ControllerBase
{
    [HttpGet("/api/user/id/{id}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<UserInfo> GetUserById(string id)
    {
        var user = UserData.Instance.GetUserById(id);
        
        if (user == null) 
            return NotFound();

        return user;
    }

    [HttpGet("/api/user/discord/{discordId}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<UserInfo> GetUserByDiscordId(string discordId)
    {
        var user = UserData.Instance.GetUserByDiscordId(discordId);

        if (user == null) 
            return NotFound();
        
        return user;
    }
}