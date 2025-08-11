using LoungeSaber_Server.Models.Client;
using LoungeSaber_Server.Models.ClientData;
using LoungeSaber_Server.SQL;
using Microsoft.AspNetCore.Mvc;

namespace LoungeSaber_Server.Api.Controllers;

[ApiController]
public class LeaderboardApiController(UserData userData) : ControllerBase
{
    [HttpGet("api/leaderboard/range/")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public UserInfo[] GetLeaderboardRange(int start, int range)
    {
        range = Math.Min(range, 10);

        var users = userData.GetAllUsers().Where(i => i.Rank >= start).ToArray();
        Array.Resize(ref users, Math.Min(users.Length, range));

        return users;
    }

    [HttpGet("/api/leaderboard/aroundUser/{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<UserInfo[]> GetAroundUser(string userId)
    {
        var users = userData.GetAllUsers();

        if (users.All(i => i?.UserId != userId))
            return NotFound();
        
        var userIdx = users.FindIndex(i => i?.UserId == userId);
        
        return users.Slice(Math.Clamp(userIdx - 4, 0, users.Count), Math.Max(users.Count, users.Count)).ToArray();
    }
}