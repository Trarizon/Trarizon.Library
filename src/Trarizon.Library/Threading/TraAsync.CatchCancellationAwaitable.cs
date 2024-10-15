using System.Runtime.CompilerServices;

namespace Trarizon.Library.Threading;
partial class TraAsync
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
                catch (TaskCanceledException) {
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

        public readonly struct Awaiter : INotifyCompletion, ICriticalNotifyCompletion
        {
            private readonly Task<T> _task;
            internal Awaiter(Task<T> task) => _task = task;

            public bool IsCompleted => _task.IsCompleted;

            public void OnCompleted(Action continuation)
                => _task.GetAwaiter().OnCompleted(continuation);
            public void UnsafeOnCompleted(Action continuation)
                => _task.GetAwaiter().UnsafeOnCompleted(continuation);

            public CancellationTaskResult<T> GetResult()
            {
                try {
                    return new CancellationTaskResult<T>(_task.GetAwaiter().GetResult());
                }
                catch (TaskCanceledException) {
                    return CancellationTaskResult<T>.Cancelled;
                }
            }
        }
    }

    public readonly struct CancellationTaskResult
    {
        public bool IsCancelled { get; }

        public static CancellationTaskResult Completed => new(false);

        public static CancellationTaskResult Cancelled => new(true);

        private CancellationTaskResult(bool cancelled) => IsCancelled = cancelled;
    }

    public readonly struct CancellationTaskResult<T>
    {
        public T Result { get; } = default!;

        internal readonly bool _hasValue;
        public bool IsCancelled => !_hasValue;

        internal CancellationTaskResult(T result)
        {
            Result = result;
            _hasValue = true;
        }

        public static CancellationTaskResult<T> Cancelled => default;
    }
}
