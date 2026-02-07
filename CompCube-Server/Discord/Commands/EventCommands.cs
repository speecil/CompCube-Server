using BeatSaverSharp.Models;
using CompCube_Models.Models.Events;
using CompCube_Server.Api.BeatSaver;
using CompCube_Server.Discord.Events;
using CompCube_Server.Gameplay.Events;
using CompCube_Server.Logging;
using CompCube_Server.SQL;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace CompCube_Server.Discord.Commands;

[SlashCommand("event", "event command")]
public class EventCommands(EventsManager eventsManager, EventFactory eventFactory, MapData mapData, Logger logger, BeatSaverApiWrapper beatSaver) : ApplicationCommandModule<ApplicationCommandContext>
{
    [SubSlashCommand("create", "creates an event")]
    public InteractionMessageProperties CreateEvent(string eventName, string displayName, string description)
    {
        if (eventsManager.ActiveEvents.FirstOrDefault(i => i.EventData.EventName == eventName) != null)
            return "Event already exists!";
        
        
        eventsManager.AddEvent(eventFactory.Create(new EventData(eventName, displayName, description, Context.User.Username, Context.User.Id,true)));
        return "Event created!";
    }

    [SubSlashCommand("startevent", "start an event")]
    public InteractionMessageProperties StartEvent(string eventName)
    {
        var e = eventsManager.ActiveEvents.FirstOrDefault(i => i.EventData.EventName == eventName);

        if (e == null)
            return $"Event {eventName} not found!";
        
        if (e.ClientCount < 2)
            return "Cannot start: Player count must be above 1!";
        
        e.StartEvent();
        return $"Event {eventName} started with {e.ClientCount} players!";
    }

    [SubSlashCommand("stop", "stops an event")]
    public InteractionMessageProperties StopEvent(string eventName)
    {
        var e = eventsManager.ActiveEvents.FirstOrDefault(i => i.EventData.EventName == eventName);

        if (e?.EventData.EventOwnerId != Context.User.Id && !Context.Guild!.IsOwner)
            return "You cannot stop this event!";
        
        if (e == null)
            return $"Event {eventName} not found!";
        
        eventsManager.RemoveEvent(e);
        
        return $"Event {eventName} stopped!";
    }

    [SubSlashCommand("setmap", "sets the map of an event")]
    public async Task<InteractionMessageProperties> SetMap(string eventName, string mapKey, string difficulty)
    {
        var e = eventsManager.ActiveEvents.FirstOrDefault(i => i.EventData.EventName == eventName);
        
        if (e == null)
            return $"Event {eventName} not found!";

        if (e.EventData.EventOwnerId != Context.User.Id && !Context.Guild!.IsOwner)
            return "You cannot set the map of this event!";
        
        var map = await beatSaver.GetBeatmapFromKey(mapKey);

        if (map == null)
            return "Could not find beatmap!";
        
        if (!Enum.TryParse<BeatmapDifficulty.BeatSaverBeatmapDifficulty>(difficulty, out var beatSaverDiff))
            return "Could not parse difficulty! (Options were Easy, Normal, Expert, and ExpertPlus. This is case sensitive!)";

        if (!map.LatestVersion.Difficulties.Any(i => i is
                                                     {
                                                         MappingExtensions: false, 
                                                         NoodleExtensions: false,
                                                         Characteristic: BeatmapDifficulty.BeatmapCharacteristic.Standard
                                                     }
                                                     && i.Difficulty == beatSaverDiff))
        {
            return $"Invalid map difficulty or characteristic!";
        }

        return $"Map set to {mapKey} ({difficulty})";
    }

    [SubSlashCommand("startmatch", "starts the map match")]
    public InteractionMessageProperties StartMatch(string eventName)
    {
        var e = eventsManager.ActiveEvents.FirstOrDefault(i => i.EventData.EventName == eventName);

        if (e == null)
            return "Event not found!";
        
        if (e.EventData.EventOwnerId != Context.User.Id && !Context.Guild!.IsOwner)
            return "You cannot start this event!";

        try
        {
            e.StartEvent();
        }
        catch (Exception ex)
        {
            logger.Error(ex);
            return $"Could not start event! Please try again later.";
        }
        
        return $"Event {eventName} started!";
    }
}