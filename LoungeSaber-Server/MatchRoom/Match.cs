using System.Globalization;
using LoungeSaber_Server.Models;
using LoungeSaber_Server.Models.Divisions;
using LoungeSaber_Server.Models.Maps;
using LoungeSaber_Server.Models.Networking;
using Newtonsoft.Json.Linq;

namespace LoungeSaber_Server.MatchRoom;

public class Match
{
    private readonly ConnectedUser UserOne;
    private readonly ConnectedUser UserTwo;

    private readonly Division Division;

    private readonly List<VotingMap> mapVotes = [];

    public event Action<Match>? MatchEnded;

    private (User user, int score)? firstScorePosted = null;
    
    public Match(ConnectedUser userOne, ConnectedUser userTwo, Division division)
    {
        UserTwo = userTwo;
        UserOne = userOne;

        Division = division;
        
        UserOne.OnUserLeftGame += OnUserLeftGame;
        UserTwo.OnUserLeftGame += OnUserLeftGame;
    }

    private async void OnUserLeftGame(User user)
    {
        try
        {
            await EndMatch("OpponentDisconnected");
        }
        catch (Exception e)
        {
            Program.LogError(e.ToString());
        }
    }

    public async Task StartMatch()
    {
        var votingOptions = Division.GetRandomMaps(3);
        
        var matchCreatedAction = new ServerPacket(ServerPacket.ActionType.CreateMatch, new JObject
        {
            {"opponent", JToken.FromObject(UserTwo.UserInfo) },
            {"votingOptions", JArray.FromObject(votingOptions) }
        });
        await UserOne.SendServerPacket(matchCreatedAction);

        matchCreatedAction.Data["opponent"] = JToken.FromObject(UserOne.UserInfo);
        await UserTwo.SendServerPacket(matchCreatedAction);
        
        UserOne.OnUserVoteRecieved += OnUserVoteRecieved;
        UserTwo.OnUserVoteRecieved += OnUserVoteRecieved;
    }

    private async void OnUserVoteRecieved(User user, VotingMap mapDifficultyVote)
    {
        try
        {
            var votingUser = user.ID == UserOne.UserInfo.ID ? UserOne : UserTwo;

            votingUser.OnUserVoteRecieved -= OnUserVoteRecieved;
        
            mapVotes.Add(mapDifficultyVote);

            var nonVotingUser = votingUser.UserInfo.ID == UserOne.UserInfo.ID ? UserTwo : UserOne;

            await nonVotingUser.SendServerPacket(new ServerPacket(ServerPacket.ActionType.OpponentVoted, new JObject
            {
                {"opponentVote", JToken.FromObject(mapDifficultyVote)}
            }));

            if (mapVotes.Count != 2) 
                return;
        
            var random = new Random();
            var randomlySelectedMap = mapVotes[random.Next(0, 3)];
            
            await EndVoting(randomlySelectedMap);
        }
        catch (Exception e)
        {
            Program.LogError(e.ToString());
            await EndMatch("ServerError");
        }
    }

    private async Task EndMatch(string reason)
    {
        await SendActionToBothUsers(new ServerPacket(ServerPacket.ActionType.MatchEnded, new JObject
        {
            {"reason", reason}
        }));
        MatchEnded?.Invoke(this);
    }

    private async Task EndVoting(VotingMap selectedMapDifficulty)
    {
        var startingTime = DateTime.UtcNow.AddSeconds(20);
        
        await SendActionToBothUsers(new ServerPacket(ServerPacket.ActionType.StartMatch, new JObject
        {
            {"selectedMap", JToken.FromObject(selectedMapDifficulty)},
            {"startingTime", startingTime.ToString("o", CultureInfo.InvariantCulture)}
        }));
        
        UserOne.OnUserScorePosted += OnUserScorePosted;
        UserTwo.OnUserScorePosted += OnUserScorePosted;
    }

    private async void OnUserScorePosted(User user, int score)
    {
        try
        {
            var scoreSubmitter = user.ID == UserOne.UserInfo.ID ? UserOne : UserTwo;
        
            scoreSubmitter.OnUserScorePosted -= OnUserScorePosted;
        
            if (firstScorePosted == null)
            {
                firstScorePosted = (user, score);
                return;
            }

            var first = user.ID == UserOne.UserInfo.ID ? UserTwo : UserOne;

            var mmrChange = CalculateMMRChange(Math.Abs(firstScorePosted!.Value.score - score));

            var resultsServerAction = new ServerPacket(ServerPacket.ActionType.Results, new JObject
            {
                {"opponentScore", firstScorePosted!.Value.score},
                {"mmrChange", mmrChange}
            });

            await scoreSubmitter.SendServerPacket(resultsServerAction);

            resultsServerAction.Data["opponentScore"] = score;
        
            await first.SendServerPacket(resultsServerAction);
        
            MatchEnded?.Invoke(this);
        }
        catch (Exception e)
        {
            Program.LogError(e.ToString());
            await EndMatch("ServerError");
        }
    }

    // TODO: add mmr calculation
    private int CalculateMMRChange(int scoreDifference)
    {
        return 0;
    }

    private async Task SendActionToBothUsers(ServerPacket serverPacket)
    {
        await UserOne.SendServerPacket(serverPacket);
        await UserTwo.SendServerPacket(serverPacket);
    }
}