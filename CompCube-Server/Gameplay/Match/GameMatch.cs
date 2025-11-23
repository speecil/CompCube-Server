using CompCube_Models.Models.ClientData;
using CompCube_Models.Models.Map;
using CompCube_Models.Models.Match;
using CompCube_Models.Models.Packets;
using CompCube_Models.Models.Packets.ServerPackets;
using CompCube_Models.Models.Packets.UserPackets;
using CompCube_Server.Discord.Events;
using CompCube_Server.Interfaces;
using CompCube_Server.Logging;
using CompCube_Server.SQL;
using CompCube.Gameplay.Match;

namespace CompCube_Server.Gameplay.Match;

public class GameMatch
{
    private readonly MatchLog _matchLog;
    private readonly UserData _userData;
    private readonly MapData _mapData;
    private readonly Logger _logger;
    private readonly MatchMessageManager _matchMessageManager;

    private Team _redTeam;
    private Team _blueTeam;

    private ScoreManager _scoreManager;
    private VoteManager _voteManager;
    
    private MatchSettings _matchSettings;

    private VotingMap? _selectedMap;

    private const int MmrLossOnDisconnect = 50;

    private int _id;
    
    private const int KFactor = 75;

    public GameMatch(MatchLog matchLog, UserData userData, MapData mapData, Logger logger, MatchMessageManager matchMessageManager)
    {
        _matchLog = matchLog;
        _userData = userData;
        _logger = logger;
        _mapData = mapData;
        _matchMessageManager = matchMessageManager;
    }

    public void Init(IConnectedClient[] redTeamPlayers, IConnectedClient[] blueTeamPlayers, MatchSettings settings)
    {
        _matchSettings = settings;
        
        _redTeam = new Team(redTeamPlayers, Team.TeamName.Red);
        _blueTeam = new Team(blueTeamPlayers, Team.TeamName.Blue);
        
        _id = _matchLog.GetValidMatchId();
        
        _redTeam.OnTeamMemberDisconnected += OnTeamMemberDisconnected;
    }

    public void StartMatch() =>
        StartMatchAsync();

    public async Task StartMatchAsync()
    {
        
    }

    private void OnTeamMemberDisconnected(int remainingCount)
    {
        
    }

    private async Task SendAllClientsPacketAsync(ServerPacket packet)
    {
        var allPlayers = _redTeam.Players.Concat(_blueTeam.Players);

        foreach (var player in allPlayers)
            await player.SendPacket(packet);
    }

    private int GetMmrChange(UserInfo winner, UserInfo loser)
    {
        if (_matchSettings.Competitive)
            return 0;
        
        var p = (1.0 / (1.0 + Math.Pow(10, ((winner.Mmr - loser.Mmr) / 400.0))));

        return (int) (KFactor * p);
    }
}