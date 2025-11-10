using CompCube_Models.Models.Events;
using CompCube_Server.Gameplay.Events;
using CompCube_Server.Logging;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace CompCube_Server.Discord.Commands;

[SlashCommand("event", "event command")]
public class EventCommands(EventsManager eventsManager) : ApplicationCommandModule<ApplicationCommandContext>
{
    [SubSlashCommand("create", "creates an event")]
    public InteractionMessageProperties CreateEvent(string eventName, string displayName, string description)
    {
        if (eventsManager.ActiveEvents.FirstOrDefault(i => i.EventData.EventName == eventName) != null)
        {
            return "Event already exists!";
        }
        
        eventsManager.AddEvent(new Event(new EventData(eventName, displayName, description, true)));
        return "Event created!";
    }

    [SubSlashCommand("start", "start an event")]
    public InteractionMessageProperties StartEvent(string eventName)
    {
        var e = eventsManager.ActiveEvents.FirstOrDefault(i => i.EventData.EventName == eventName);

        if (e == null)
            return $"Event {eventName} not found!";
        
        if (e.ClientCount == 0)
            return "Cannot start: Player count must be above 0!";

        if (e.ClientCount % 2 == 1)
            return "Cannot start: Player count must be even!";
        
        e.StartEvent();
        return $"Event {eventName} started with {e.ClientCount} players!";
    }

    [SubSlashCommand("stop", "stops an event")]
    public InteractionMessageProperties StopEvent(string eventName)
    {
        var e = eventsManager.ActiveEvents.FirstOrDefault(i => i.EventData.EventName == eventName);
        
        if (e == null)
            return $"Event {eventName} not found!";
        
        
        
        return $"Event {eventName} stopped!";
    }
}