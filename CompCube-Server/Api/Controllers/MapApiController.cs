using CompCube_Server.SQL;
using Microsoft.AspNetCore.Mvc;

namespace CompCube_Server.Api.Controllers;

[ApiController]
public class MapApiController(MapData mapData) : ControllerBase
{
    [HttpGet("/api/maps/hashes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<string[]> GetAllMapHashes() => mapData.GetAllMaps().Select(i => i.Hash).ToArray();
}