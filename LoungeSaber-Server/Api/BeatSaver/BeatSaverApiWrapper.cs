using BeatSaverSharp;
using BeatSaverSharp.Models;

namespace LoungeSaber_Server.BeatSaverApi;

public class BeatSaverApiWrapper
{
    private readonly BeatSaver _beatSaver = new(new BeatSaverOptions("LoungeSaber-Server", new Version("1.0.0")));

    public async Task<Beatmap?> GetBeatmapFromHash(string hash) => await _beatSaver.BeatmapByHash(hash);
}