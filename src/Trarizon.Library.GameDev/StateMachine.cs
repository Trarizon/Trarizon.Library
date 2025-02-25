namespace Trarizon.Library.GameDev;
public class StateMachine
{
    private IState? _state;

    public IState? CurrentState
    {
        get => _state;
        set {
            if (_state == value)
                return;
            _state?.Exit();
            _state = value;
            _state?.Enter();
        }
    }

    public void Update()
    {
        if (_state is not null) {
            _state.Update();
        }
    }
}

public interface IState
{
    void Enter();
    void Exit();
    void Update();
}

public interface IAsyncEnterState : IState
{
    Task OnEnterAsync(CancellationToken cancellationToken);
}

internal sealed class DelegateState(StateMachine stateMachine, Action<StateMachine>? enter, Action? exit, Action? update) : IState
{
    private readonly CancellationToken _cts;

    public CancellationToken CancellationToken => _cts;

    public DelegateState(StateMachine stateMachine, CancellationToken cancellationToken, Func<StateMachine, Task> enterAsync, Action? exit, Action? update) :
        this(stateMachine,enterAsync.AsSync(), exit, update)
    { }

    public void Enter() => enter?.Invoke(stateMachine);
    public void Exit() => exit?.Invoke();
    public void Update() => update?.Invoke();
}
