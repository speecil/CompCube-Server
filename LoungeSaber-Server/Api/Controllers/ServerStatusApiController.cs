using LoungeSaber_Server.Models.Server;
using LoungeSaber_Server.ServerState;
using Microsoft.AspNetCore.Mvc;

namespace LoungeSaber_Server.Api.Controllers;

[ApiController]
public class ServerStatusApiController : ControllerBase
{
    //TODO: change these to be read from config file at some point
    //TODO: make the server stop accepting requests if the server is not in online mode
    [HttpGet("/api/server/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<ServerStatus> GetServerStatus() => new ServerStatus(["1.39.1"], ["1.0.0"], ServerStateController.State);
}