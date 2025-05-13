using LoungeSaber_Server.SQL;

namespace LoungeSaber_Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                Database.Start();
                Api.Api.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                Database.Stop();
            }
        }
    }
}