using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library.Components;
[Experimental("TRASM")]
public interface IStateMachineState
{
    void Enter();
    void Exit();
    void Invoke();
}

[Experimental("TRASM")]
public sealed class StateMachine
{
    private IStateMachineState? _state;

    public IStateMachineState? CurrentState
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

    public void Invoke()
    {
        if (_state is not null) {
            _state.Invoke();
        }
    }
}
