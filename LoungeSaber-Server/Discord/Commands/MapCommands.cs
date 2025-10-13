using LoungeSaber_Server.BeatSaverApi;
using LoungeSaber_Server.Models.Map;
using LoungeSaber_Server.SQL;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace LoungeSaber_Server.Discord.Commands;

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

        if (!Enum.TryParse<VotingMap.CategoryType>(category, out var mapCategory))
            return "Could not parse category!";
        
        mapData.AddMap(new VotingMap(beatmap.LatestVersion.Hash, difficulty, mapCategory));

        return $"{beatmap.Name} added to pool.";
    }
}