using System.Runtime.CompilerServices;
using Trarizon.Library.Wrappers;

namespace Trarizon.Library.Threading;
public static partial class TraAsync
{
    /// <summary>
    /// Returns a awaitable that will catch <see cref="TaskCanceledException"/>
    /// </summary>
    public static CatchCancellationAwaitable CatchCancellation(this Task task) => new(task);

    /// <summary>
    /// Returns a awaitable that will catch <see cref="TaskCanceledException"/>
    /// </summary>
    public static CatchCancellationAwaitable<T> CatchCancellation<T>(this Task<T> task) => new(task);

    public readonly struct CatchCancellationAwaitable
    {
        private readonly Awaiter _awaiter;

        internal CatchCancellationAwaitable(Task task) => _awaiter = new Awaiter(task);

        public Awaiter GetAwaiter() => _awaiter;

        public async Task AsTask()
        {
            try { await this; }
            catch (OperationCanceledException) { return; }
        }

        public readonly struct Awaiter : INotifyCompletion, ICriticalNotifyCompletion
        {
            private readonly Task _task;
            internal Awaiter(Task task) => _task = task;

            public bool IsCompleted => _task.IsCompleted;

            public void OnCompleted(Action continuation)
                => _task.GetAwaiter().OnCompleted(continuation);
            public void UnsafeOnCompleted(Action continuation)
                => _task.GetAwaiter().UnsafeOnCompleted(continuation);

            public CancellationTaskResult GetResult()
            {
                try {
                    _task.GetAwaiter().GetResult();
                    return CancellationTaskResult.Completed;
                }
                catch (OperationCanceledException) {
                    return CancellationTaskResult.Cancelled;
                }
            }
        }
    }

    public readonly struct CatchCancellationAwaitable<T>
    {
        private readonly Awaiter _awaiter;

        internal CatchCancellationAwaitable(Task<T> task) => _awaiter = new Awaiter(task);

        public Awaiter GetAwaiter() => _awaiter;

        public async Task<T?> AsTask()
        {
            try { return (await this).Value; }
            catch (OperationCanceledException) { return default; }
        }

        public readonly struct Awaiter : INotifyCompletion, ICriticalNotifyCompletion
        {
            private readonly Task<T> _task;
            internal Awaiter(Task<T> task) => _task = task;

            public bool IsCompleted => _task.IsCompleted;

            public void OnCompleted(Action continuation)
                => _task.GetAwaiter().OnCompleted(continuation);
            public void UnsafeOnCompleted(Action continuation)
                => _task.GetAwaiter().UnsafeOnCompleted(continuation);

            public Optional<T> GetResult()
            {
                try {
                    return Optional.Of(_task.GetAwaiter().GetResult());
                }
                catch (OperationCanceledException) {
                    return default;
                }
            }
        }
    }

    public readonly struct CancellationTaskResult
    {
        public bool IsSuccess { get; }

        public static CancellationTaskResult Completed => new(true);

        public static CancellationTaskResult Cancelled => new(false);

        private CancellationTaskResult(bool success) => IsSuccess = success;
    }
}
