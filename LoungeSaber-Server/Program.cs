using LoungeSaber_Server.Discord;
using LoungeSaber_Server.Gameplay.Matchmaking;
using LoungeSaber_Server.SQL;

namespace LoungeSaber_Server
{
    public class Program
    {
        public const bool Debug = true;
        
        public static void Main(string[] args)
        {
            try
            {
                UserData.Instance.Start();
                MapData.Instance.Start();
                ConnectionManager.Start();
                DiscordBot.RunDiscordBot();
                Api.Api.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}