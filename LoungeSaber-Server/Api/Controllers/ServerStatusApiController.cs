using LoungeSaber_Server.Models.Server;
using LoungeSaber_Server.ServerMaintenanceState;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace LoungeSaber_Server.Api.Controllers;

[ApiController]
public class ServerStatusApiController : ControllerBase
{
    //TODO: change these to be read from config file at some point
    //TODO: make the server stop accepting join requests if the server is not in online mode
    [HttpGet("/api/server/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<string> GetServerStatus() => JsonConvert.SerializeObject(ServerStatus.GetServerMaintenanceState());
}