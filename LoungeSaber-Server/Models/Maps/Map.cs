namespace LoungeSaber_Server.Models.Maps;

public class Map(string hash, List<MapDifficulty> difficulties)
{
    public string Hash { get; } = hash;
    
    public List<MapDifficulty> Difficulties { get; } = difficulties;

    public PlaylistMap GetPlaylistMap() => new(Hash, Difficulties.ToArray());

    public VotingMap ToVotingMap(MapDifficulty difficulty) => new(Hash, difficulty);
}