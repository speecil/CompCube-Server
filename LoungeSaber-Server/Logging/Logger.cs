using System.Runtime.CompilerServices;

namespace LoungeSaber_Server.Logging;

public class Logger
{
    public void Info(string text, [CallerFilePath] string callerPath = "") => Log(text, callerPath, ConsoleColor.White);

    public void Error(string text, [CallerFilePath] string callerPath = "") => Log(text, callerPath, ConsoleColor.Red);

    public void Error(Exception ex, [CallerFilePath] string callerPath = "") => Error(ex.ToString());
    
    private void Log(string text, string callerFilePath, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine($"[{Path.GetFileNameWithoutExtension(callerFilePath)} {DateTime.UtcNow.ToShortDateString()} {DateTime.UtcNow.ToShortTimeString()}] {text}");
        Console.ForegroundColor = ConsoleColor.White;
    }
}