using System.Timers;
using LoungeSaber_Server.Interfaces;
using Timer = System.Timers.Timer;

namespace LoungeSaber_Server.Models.Client;

public class MatchmakingClient(IConnectedClient client)
{
    public IConnectedClient Client { get; private set; } = client;

    public int MmrThreshold { get; private set; } = 100;

    private void OnMmrThresholdClockElapsed(object? sender, ElapsedEventArgs e)
    {
        MmrThreshold += 500;
    }

    public bool CanMatchWithOtherClient(MatchmakingClient other)
    {
        return Math.Abs(other.MmrThreshold - MmrThreshold) <= MmrThreshold;
    }
}