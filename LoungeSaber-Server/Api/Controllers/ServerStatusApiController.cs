using LoungeSaber_Server.Models.Server;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace LoungeSaber_Server.Api.Controllers;

[ApiController]
public class ServerStatusApiController(ServerStatusManager serverStatusManager) : ControllerBase
{
    //TODO: change these to be read from config file at some point
    [HttpGet("/api/server/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<string> GetServerStatus() => JsonConvert.SerializeObject(serverStatusManager.GetServerStatus());
}