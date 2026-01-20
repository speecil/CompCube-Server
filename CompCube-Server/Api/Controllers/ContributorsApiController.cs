using CompCube_Server.Models.CompCube_Models.Models.Contributors;
using Microsoft.AspNetCore.Mvc;

namespace CompCube_Server.Api.Controllers;

[ApiController]
public class ContributorsApiController(IConfiguration config) : ControllerBase
{
    [HttpGet("api/contributors")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<Contributor[]> GetContributors()
    {
        var contributors = config.GetSection("contributors").Get<Contributor[]>();

        if (contributors == null)
            throw new Exception("Could not parse contributors from file!");

        return contributors;
    }
}