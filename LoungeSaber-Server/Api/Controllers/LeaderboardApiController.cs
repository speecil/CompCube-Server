using LoungeSaber_Server.Models.Client;
using LoungeSaber_Server.SQL;
using Microsoft.AspNetCore.Mvc;

namespace LoungeSaber_Server.Api.Controllers;

[ApiController]
public class LeaderboardApiController : ControllerBase
{
    [HttpGet("api/leaderboard/range/")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<UserInfo[]> GetLeaderboardRange(int start, int range)
    {
        range = Math.Min(range, 10);

        var users = UserData.Instance.GetAllUsers().Where(i => i.Rank >= start).ToArray();
        Array.Resize(ref users, Math.Min(users.Length, range));

        return users;
    }
}