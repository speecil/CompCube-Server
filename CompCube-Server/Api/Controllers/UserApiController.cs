using CompCube_Models.Models.ClientData;
using CompCube_Server.SQL;
using Microsoft.AspNetCore.Mvc;

namespace CompCube_Server.Api.Controllers;

[ApiController]
public class UserApiController(UserData userData) : ControllerBase
{
    [HttpGet("/api/user/id/{id}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<UserInfo> GetUserById(string id)
    {
        var user = userData.GetUserById(id);
        
        if (user == null) 
            return NotFound();

        return user;
    }

    [HttpGet("/api/user/discord/{discordId}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<UserInfo> GetUserByDiscordId(string discordId)
    {
        var user = userData.GetUserByDiscordId(discordId);

        if (user == null) 
            return NotFound();
        
        return user;
    }
}