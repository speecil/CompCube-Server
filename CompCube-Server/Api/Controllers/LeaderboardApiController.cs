using CompCube_Models.Models.ClientData;
using CompCube_Server.SQL;
using Microsoft.AspNetCore.Mvc;

namespace CompCube_Server.Api.Controllers;

[ApiController]
public class LeaderboardApiController(UserData userData) : ControllerBase
{
    [HttpGet("api/leaderboard/range/")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<UserInfo[]> GetLeaderboardRange(int start, int range) =>
        userData.GetLeaderboardRange(start, range);

    [HttpGet("/api/leaderboard/aroundUser/{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<UserInfo[]> GetAroundUser(string userId)
    {
        var aroundUser = userData.GetAroundUser(userId);

        if (aroundUser == null)
            return NotFound();

        return aroundUser;
    }
}