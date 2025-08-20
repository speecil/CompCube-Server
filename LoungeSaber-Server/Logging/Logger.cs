using System.Runtime.CompilerServices;

namespace LoungeSaber_Server.Logging;

public class Logger
{
    public void Info(string text, [CallerFilePath] string callerPath = "") => Log(text, callerPath, ConsoleColor.White);

    public void Error(string text, [CallerFilePath] string callerPath = "") => Log(text, callerPath, ConsoleColor.Red);

    public void Error(Exception ex, [CallerFilePath] string callerPath = "") => Error(ex.ToString());
    
    private void Log(string text, string callerFilePath, ConsoleColor color)
    {
        // rider doesn't like it when you change the console color to white
        // this works fine though
        if (color == ConsoleColor.White) 
            Console.ResetColor();
        
        Console.WriteLine($"[{Path.GetFileNameWithoutExtension(callerFilePath)} {DateTime.UtcNow.ToShortDateString()} {DateTime.UtcNow.ToShortTimeString()}] {text}");
        Console.ResetColor();
    }
}