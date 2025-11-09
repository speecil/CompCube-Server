using CompCube_Models.Models.Map;
using CompCube_Server.Api.BeatSaver;
using CompCube_Server.SQL;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace CompCube_Server.Discord.Commands;

public class MapCommands(BeatSaverApiWrapper beatSaverApi, MapData mapData) : ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand("addmap", "add a map")]
    public async Task<InteractionMessageProperties> AddMap(string key, string diff, string category)
    {
        var beatmap = await beatSaverApi.GetBeatmapFromKey(key);

        if (beatmap == null) 
            return "Invalid key!";

        if (!Enum.TryParse<VotingMap.DifficultyType>(diff, out var difficulty))
            return "Could not parse difficulty!";
        
        mapData.AddMap(new VotingMap(beatmap.LatestVersion.Hash, difficulty, category));

        return $"{beatmap.Name} added to pool.";
    }
}