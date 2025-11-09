using System.Data;
using System.Data.SQLite;

namespace CompCube_Server.SQL;

public abstract class Database : IDisposable
{
    protected abstract string DatabaseName { get; }
    
    protected readonly SQLiteConnection Connection;

    protected static string DataFolderPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");

    public bool IsOpen => Connection.State == ConnectionState.Open;

    protected Database()
    {
        Connection = new($"Data Source={Path.Combine(DataFolderPath, $"{DatabaseName}.db;")}");
        
        Start();
    }

    private void Start()
    {
        if (IsOpen) 
            return;

        Directory.CreateDirectory(DataFolderPath);
        
        Connection.Open();
        CreateInitialTables();
    }
    
    public void Stop()
    {
        if (!IsOpen) 
            return;
        
        Connection.Close();
    }

    protected abstract void CreateInitialTables();

    public void Dispose() => Stop();
}