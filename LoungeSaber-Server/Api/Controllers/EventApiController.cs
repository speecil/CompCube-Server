using LoungeSaber_Server.Gameplay.Events;
using LoungeSaber_Server.Models.Events;
using Microsoft.AspNetCore.Mvc;

namespace LoungeSaber_Server.Api.Controllers;

[ApiController]
public class EventApiController(EventManager eventManager) : ControllerBase
{
    [HttpGet("api/events/events")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<EventData[]> GetEvents() => eventManager.ActiveEvents.Select(i => i.EventData).ToArray();
}