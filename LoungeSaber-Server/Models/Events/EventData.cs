namespace LoungeSaber_Server.Models.Events;

public class EventData(string eventName, string displayName, string description)
{
    public string Name => eventName;
    public string DisplayName => displayName;
    public string Description => description;
}