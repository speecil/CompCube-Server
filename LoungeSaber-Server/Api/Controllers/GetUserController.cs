using LoungeSaber_Server.Models;
using LoungeSaber_Server.SQL;
using Microsoft.AspNetCore.Mvc;

namespace LoungeSaber_Server.Api.Controllers;

[ApiController]
[Route("api/getuser")]
public class GetUserController : ControllerBase
{
    [HttpGet("{userId}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<User> GetUserFromId(string userId)
    {
        var user = Database.GetUser(userId);

        if (user == null) return NotFound();
        return user;
    }
}