using System.Data;
using System.Data.SQLite;

namespace LoungeSaber_Server.SQL;

public abstract class Database
{
    protected abstract string DatabaseName { get; }
    
    protected readonly SQLiteConnection _connection;

    protected static string DataFolderPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");

    public bool IsOpen => _connection.State == ConnectionState.Open;

    protected Database()
    {
        _connection = new($"Data Source={Path.Combine(DataFolderPath, $"{DatabaseName}.db;")}");
    }

    public void Start()
    {
        if (IsOpen) 
            return;

        Directory.CreateDirectory(DataFolderPath);
        
        _connection.Open();
        CreateInitialTables();
    }
    
    public void Stop()
    {
        if (!IsOpen) 
            return;
        
        _connection.Close();
    }

    protected abstract void CreateInitialTables();
}