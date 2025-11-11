using CompCube_Models.Models.Events;
using CompCube_Models.Models.Match;
using CompCube_Server.Discord.Events;
using CompCube_Server.Logging;
using Newtonsoft.Json;

namespace CompCube_Server.Gameplay.Events;

public class EventsManager
{
    private readonly Logger _logger;
    private readonly EventMessageManager _eventMessageManager;
    
    private readonly List<Event> _events;
    
    public IReadOnlyList<Event> ActiveEvents => _events;
    
    private static string PathToEventsFile => Path.Combine(Directory.GetCurrentDirectory(), "events.json");
    
    public EventsManager(Logger logger, EventMessageManager eventMessageManager)
    {
        _logger = logger;
        _eventMessageManager = eventMessageManager;
        
        _events = ReadEventsFromFile().Select(i => new Event(i, eventMessageManager)).ToList();

        AppDomain.CurrentDomain.ProcessExit += (sender, args) =>
        {
            SaveEventsToFile();
        };
    }

    public void AddEvent(Event e)
    {
        _events.Add(e);
        
        _logger.Info($"Event {e.QueueName} has been created.");
    }

    public void RemoveEvent(Event e)
    {
        _events.Remove(e);
        
        _logger.Info($"Event {e.QueueName} has been removed.");
    }
    
    private void SaveEventsToFile()
    {
        File.WriteAllText(PathToEventsFile, JsonConvert.SerializeObject(_events));
    }

    private EventData[] ReadEventsFromFile()
    {
        if (!File.Exists(PathToEventsFile))
            return [];
        
        var deserializedEventData = JsonConvert.DeserializeObject<EventData[]>(File.ReadAllText(PathToEventsFile));

        if (deserializedEventData != null) 
            return deserializedEventData;
        
        _logger.Info("Could not read events from file.");
        return [];
    }
}