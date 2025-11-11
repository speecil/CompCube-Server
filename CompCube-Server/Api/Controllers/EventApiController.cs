using CompCube_Models.Models.Events;
using CompCube_Server.Gameplay.Events;
using Microsoft.AspNetCore.Mvc;

namespace CompCube_Server.Api.Controllers;

[ApiController]
public class EventApiController(EventsManager eventsManager) : ControllerBase
{
    [HttpGet("api/events/events")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<EventData[]> GetEvents() => eventsManager.ActiveEvents.Select(i => i.EventData).ToArray();
}