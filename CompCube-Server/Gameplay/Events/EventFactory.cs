using EventData = CompCube_Models.Models.Events.EventData;

namespace CompCube_Server.Gameplay.Events;

public class EventFactory(IServiceProvider services)
{
    public Event Create(EventData eventData)
    {
        var e = ActivatorUtilities.CreateInstance<Event>(services);

        return e;
    }
}