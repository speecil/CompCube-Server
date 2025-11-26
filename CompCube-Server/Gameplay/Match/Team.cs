using CompCube_Models.Models.ClientData;
using CompCube_Models.Models.Match;
using CompCube_Models.Models.Packets;
using CompCube_Models.Models.Packets.UserPackets;
using CompCube_Server.Interfaces;
using CompCube.Gameplay.Match;

namespace CompCube_Server.Gameplay.Match;

public class Team(IConnectedClient[] players, Team.TeamName teamName)
{
    public readonly TeamName Name = teamName;

    public TeamData TeamData => new(teamName, Players.Select(i => i.UserInfo).ToArray());

    public readonly int AverageMmr = players.Sum(i => i.UserInfo.Mmr) / 3;
    
    public List<IConnectedClient> Players { get; private set; } = players.ToList();

    private Action<TeamScores>? _onTeamFinishedCallback = null;

    private Dictionary<UserInfo, Score?> _scores = new();

    public event Action<TeamData, UserInfo, int>? OnTeamMemberDisconnected;
    
    public void DoForEach(Action<IConnectedClient>? action)
    {
        foreach (var player in Players)
            action?.Invoke(player);
    }

    public async Task SendPacketToAll(ServerPacket packet)
    {
        foreach (var player in Players)
            await player.SendPacket(packet);
    }

    public void HandleMatchStarting(Action<TeamScores> onTeamFinishedCallback)
    {
        _onTeamFinishedCallback = onTeamFinishedCallback;

        _scores = players.Select(i => new KeyValuePair<UserInfo, Score?>(i.UserInfo, null)).ToDictionary();
        
        DoForEach(i =>
        {
            i.OnScoreSubmission += HandleScoreSubmitted;
            i.OnDisconnected += HandleClientDisconnected;
        });
    }

    private void HandleClientDisconnected(IConnectedClient client)
    {
        client.OnDisconnected -= HandleClientDisconnected;
        client.OnScoreSubmission -= HandleScoreSubmitted;

        Players.Remove(client);
        
        _scores.Remove(client.UserInfo);

        OnTeamMemberDisconnected?.Invoke(TeamData, client.UserInfo, Players.Count);
        
        CheckIfTeamIsWaitingOnScore();
    }

    private void HandleScoreSubmitted(ScoreSubmissionPacket packet, IConnectedClient client)
    {
        client.OnScoreSubmission -= HandleScoreSubmitted;

        _scores[client.UserInfo] = packet.GetScore();
        
        CheckIfTeamIsWaitingOnScore();
    }

    private void CheckIfTeamIsWaitingOnScore()
    {
        if (_scores.Any(i => i.Value == null))
            return;
        
        _onTeamFinishedCallback?.Invoke(new TeamScores(Name, _scores.Select(i => new MatchScore(i.Key, i.Value)).ToArray()));
        _onTeamFinishedCallback = null;
    }

    public enum TeamName
    {
        Red,
        Blue
    }
}