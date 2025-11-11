using CompCube_Models.Models.Events;
using CompCube_Server.Discord.Events;
using CompCube_Server.Gameplay.Events;
using CompCube_Server.Logging;
using CompCube_Server.SQL;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace CompCube_Server.Discord.Commands;

[SlashCommand("event", "event command")]
public class EventCommands(EventsManager eventsManager, EventMessageManager eventMessageManager, MapData mapData) : ApplicationCommandModule<ApplicationCommandContext>
{
    [SubSlashCommand("create", "creates an event")]
    public InteractionMessageProperties CreateEvent(string eventName, string displayName, string description)
    {
        if (eventsManager.ActiveEvents.FirstOrDefault(i => i.EventData.EventName == eventName) != null)
        {
            return "Event already exists!";
        }
        
        eventsManager.AddEvent(new Event(new EventData(eventName, displayName, description, true), eventMessageManager));
        return "Event created!";
    }

    [SubSlashCommand("startEvent", "start an event")]
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
        
        //TODO: implement
        
        return $"Event {eventName} stopped!";
    }

    [SubSlashCommand("setMap", "sets the map of an event")]
    public InteractionMessageProperties SetMap(string eventName, string mapId)
    {
        var e = eventsManager.ActiveEvents.FirstOrDefault(i => i.EventData.EventName == eventName);
        
        if (e == null)
            return $"Event {eventName} not found!";
        
        //todo: map ids
        var map = mapData.GetAllMaps().First();
        
        e.SetMap(map);

        return $"Map set to {mapId}";
    }

    [SubSlashCommand("startMatch", "starts the map match")]
    public InteractionMessageProperties StartMatch(string eventName)
    {
        var e = eventsManager.ActiveEvents.FirstOrDefault(i => i.EventData.EventName == eventName);

        if  (e == null)
            return "Event not found!";

        try
        {
            e.StartEvent();
        }
        catch (Exception ex)
        {
            return $"Could not start event: {ex}";
        }
        
        return $"Event {eventName} started!";
    }
}