using Microsoft.AspNetCore.Mvc;

namespace CompCube_Server.Api.Controllers;

[ApiController]
public class ContributorsApiController : ControllerBase
{
    [HttpGet("api/contributors")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<>
}