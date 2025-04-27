using System.Runtime.CompilerServices;

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

            public CatchCancellationTaskResult GetResult()
            {
                try {
                    _task.GetAwaiter().GetResult();
                    return CatchCancellationTaskResult.Completed;
                }
                catch (OperationCanceledException) {
                    return CatchCancellationTaskResult.Cancelled;
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

            public CatchCancellationTaskResult<T> GetResult()
            {
                try {
                    return new CatchCancellationTaskResult<T>(_task.GetAwaiter().GetResult());
                }
                catch (OperationCanceledException) {
                    return default;
                }
            }
        }
    }

    public readonly struct CatchCancellationTaskResult
    {
        public bool IsSuccess { get; }

        public static CatchCancellationTaskResult Completed => new(true);

        public static CatchCancellationTaskResult Cancelled => new(false);

        private CatchCancellationTaskResult(bool success) => IsSuccess = success;
    }

    public readonly struct CatchCancellationTaskResult<T>
    {
        public bool IsSuccess { get; }
        public T Value { get; }

        internal CatchCancellationTaskResult(T value)
        {
            IsSuccess = true;
            Value = value;
        }
    }
}
