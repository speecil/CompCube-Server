using CompCube_Models.Models.Server;

namespace CompCube_Server.Networking.ServerStatus;

public class ServerStatusManager
{
    private ServerState.State _state = ServerState.State.Maintenance;

    public ServerState.State State
    {
        get => _state;
        set
        {
            _state = value;
            OnStateChanged?.Invoke(value);
        }
    }
    
    public event Action<ServerState.State>? OnStateChanged;
    
    public CompCube_Models.Models.Server.ServerStatus GetServerStatus() => new(["1.39.1", "1.40.8", "1.40.5"], ["1.0.0"], State);
}