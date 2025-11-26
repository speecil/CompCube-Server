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
using VotePacket = CompCube_Models.Models.Packets.ServerPackets.VotePacket;

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
    }

    public void StartMatch() =>
        StartMatchAsync();

    public async Task StartMatchAsync()
    {
        _redTeam.OnTeamMemberDisconnected += HandleTeamMemberDisconnected;
        _blueTeam.OnTeamMemberDisconnected += HandleTeamMemberDisconnected;
        
        _voteManager.OnMapDetermined += HandleMapDetermined;
        _voteManager.OnMapVotedFor += HandleMapVotedFor;

        await SendAllClientsPacketAsync(new MatchCreatedPacket(_voteManager.VotingOptions, _redTeam.TeamData, _blueTeam.TeamData, 30));
    }

    private async void HandleMapVotedFor(IConnectedClient client, int mapIndex)
    {
        try
        {
            await SendAllClientsPacketAsync(new VotePacket(mapIndex, client.UserInfo.UserId));
        }
        catch (Exception e)
        {
            _logger.Error(e);
        }
    }

    private async void HandleMapDetermined(VotingMap map)
    {
        try
        {
            await SendAllClientsPacketAsync(new MatchStartedPacket(map, 15, 25, _redTeam.TeamData, _blueTeam.TeamData));
        }
        catch (Exception e)
        {
            _logger.Error(e);
        }
    }

    private async void HandleTeamMemberDisconnected(TeamData teamData, UserInfo userInfo, int remainingCount)
    {
        
        
        if (remainingCount == 0)
        {
            await EndMatchPrematurely("Opposing player(s) disconnected!");
            return;
        }

        await SendAllClientsPacketAsync(new UserDisconnectedPacket(userInfo.UserId));
    }

    private async Task EndMatchPrematurely(string reason)
    {
        if (_matchSettings.Competitive)
        {
            var winningTeam = _blueTeam.Players.Count > 0 ? _redTeam : _blueTeam;
            var losingTeam = winningTeam.Name == Team.TeamName.Red ? _blueTeam : _redTeam;

            var mmrChange = GetMmrChange(winningTeam, losingTeam);
            
            winningTeam.DoForEach(i =>  _userData.ApplyMmrChange(i.UserInfo, mmrChange));
        }

        await SendAllClientsPacketAsync(new PrematureMatchEndPacket(reason));
    }

    private async Task SendAllClientsPacketAsync(ServerPacket packet)
    {
        var allPlayers = _redTeam.Players.Concat(_blueTeam.Players);

        foreach (var player in allPlayers)
            await player.SendPacket(packet);
    }

    private int GetMmrChange(Team winningTeam, Team losingTeam)
    {
        if (_matchSettings.Competitive)
            return 0;
        
        var p = (1.0 / (1.0 + Math.Pow(10, ((winningTeam.AverageMmr - losingTeam.AverageMmr) / 400.0))));

        return (int) (KFactor * p);
    }
}