using System.Runtime.CompilerServices;

namespace Trarizon.Library.Playground.Threading;

[AsyncMethodBuilder(typeof(AsyncSteppedTaskMethodBuilder))]
class SteppedTask
{
    private static async SteppedTask RunAsync()
    {
        await Step(1);

        await Task.Yield();
    }

    public Awaiter GetAwaiter() => new(this);

    public struct Awaiter(SteppedTask task) : INotifyCompletion
    {
        public bool IsCompleted => false;
        public void GetResult() { }
        public void OnCompleted(Action continuation) => continuation();
    }

    public static ISteppedTaskStep<Awaiter> Step<T>(T value) { return default; }

    public class StepTask : ISteppedTaskStep<StepTask.Awaiter>
    {
        public Awaiter GetAwaiter() => new(this);

        public struct Awaiter(StepTask task) : INotifyCompletion
        {
            public void OnCompleted(Action continuation)
            {

            }
        }
    }
}

interface ISteppedTaskStep<TAwaiter>
{
    TAwaiter GetAwaiter();
}

interface ISteppedTaskStepAwaiter<TSelf> : INotifyCompletion
{
    ISteppedTaskStep<TSelf> Task { get; }
}

struct AsyncSteppedTaskMethodBuilder
{
    public static AsyncSteppedTaskMethodBuilder Create() => default;
    public SteppedTask Task => default;
    public void SetException(Exception exception) { }
    public void SetResult() { }
    public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : INotifyCompletion
        where TStateMachine : IAsyncStateMachine
    {
        if (typeof(TAwaiter) == typeof(SteppedTask.StepTask.Awaiter))
        {

        }
    }
    public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : ICriticalNotifyCompletion
        where TStateMachine : IAsyncStateMachine
    { }
    public void Start<TStateMachine>(ref TStateMachine stateMachine)
        where TStateMachine : IAsyncStateMachine
    { }
    public void SetStateMachine(IAsyncStateMachine stateMachine)
    { }
}