using BeatSaverSharp;
using BeatSaverSharp.Models;

namespace CompCube_Server.Api.BeatSaver;

public class BeatSaverApiWrapper
{
    private readonly BeatSaverSharp.BeatSaver _beatSaver = new(new BeatSaverOptions("CompCube-Server", new Version("1.0.0")));

    public async Task<Beatmap?> GetBeatmapFromHash(string hash) => await _beatSaver.BeatmapByHash(hash);

    public async Task<Beatmap?> GetBeatmapFromKey(string key) => await _beatSaver.Beatmap(key);
}