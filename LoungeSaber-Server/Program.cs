using LoungeSaber_Server.Gameplay.Matchmaking;
using LoungeSaber_Server.SQL;

namespace LoungeSaber_Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                UserData.Instance.Start();
                MapData.Instance.Start();
                ConnectionManager.Start();
                Api.Api.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }
    }
}