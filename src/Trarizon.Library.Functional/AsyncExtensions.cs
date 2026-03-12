using System.Runtime.CompilerServices;

namespace Trarizon.Library.Functional;

#if EXT_TASK

public static class AsyncExtensions
{
    public static Task<Optional<T>> Transpose<T>(this Optional<Task<T>> self)
    {
        if (self.HasValue) {
            var task = self.Value;
            if (task.IsCompletedSuccessfully) {
                return Task.FromResult(Optional.Of(task.Result));
            }
            else {
                return Continue(task);
            }
        }
        return Task.FromResult(Optional<T>.None);

        static async Task<Optional<T>> Continue(Task<T> task) => Optional.Of(await task.ConfigureAwait(false));
    }

    public static Task<Result<T, TError>> Transpose<T, TError>(this Result<Task<T>, TError> self)
    {
        if (self.IsSuccess) {
            var task = self.Value;
            if (task.IsCompletedSuccessfully) {
                return Task.FromResult(Result.Success<T, TError>(task.Result));
            }
            else {
                return Continue(task);
            }
        }
        return Task.FromResult(Result.Failure<T, TError>(self.Error));

        static async Task<Result<T, TError>> Continue(Task<T> task) => Result.Success<T, TError>(await task.ConfigureAwait(false));
    }

#if !NETSTANDARD2_0

    public static ValueTask<Optional<T>> Transpose<T>(this Optional<ValueTask<T>> self)
    {
        if (self.HasValue)
            return Continue(self.Value);
        return ValueTask.FromResult(Optional<T>.None);

        static async ValueTask<Optional<T>> Continue(ValueTask<T> task) => Optional.Of(await task.ConfigureAwait(false));
    }

    public static ValueTask<Result<T, TError>> Transpose<T, TError>(this Result<ValueTask<T>, TError> self)
    {
        if (self.IsSuccess)
            return Continue(self.Value);
        return ValueTask.FromResult(Result.Failure<T, TError>(self.Error));

        static async ValueTask<Result<T, TError>> Continue(ValueTask<T> task) => Result.Success<T, TError>(await task.ConfigureAwait(false));
    }

#endif

    public static OptionalTaskAwaiter GetAwaiter(this Optional<Task> self)
        => self.HasValue ? new(self.Value.GetAwaiter()) : default;

    public static OptionalTaskAwaiter<T> GetAwaiter<T>(this Optional<Task<T>> self)
        => self.HasValue ? new(self.Value.GetAwaiter()) : default;

#if !NETSTANDARD2_0

    public static OptionalValueTaskAwaiter GetAwaiter(this Optional<ValueTask> self)
        => self.HasValue ? new(self.Value.GetAwaiter()) : default;

    public static OptionalValueTaskAwaiter<T> GetAwaiter<T>(this Optional<ValueTask<T>> self)
        => self.HasValue ? new(self.Value.GetAwaiter()) : default;

#endif

    public readonly struct OptionalTaskAwaiter : ICriticalNotifyCompletion
    {
        private readonly Optional<TaskAwaiter> _awaiter;

        internal OptionalTaskAwaiter(TaskAwaiter taskAwaiter)
        {
            _awaiter = taskAwaiter;
        }

        public bool IsCompleted => !_awaiter.HasValue || _awaiter.Value.IsCompleted;

        public void GetResult()
        {
            if (_awaiter.HasValue)
                _awaiter.Value.GetResult();
        }

        public void OnCompleted(Action continuation)
        {
            if (_awaiter.HasValue) {
                _awaiter.Value.OnCompleted(continuation);
            }
            else {
                continuation();
            }
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            if (_awaiter.HasValue) {
                _awaiter.Value.UnsafeOnCompleted(continuation);
            }
            else {
                continuation();
            }
        }
    }

    public readonly struct OptionalTaskAwaiter<T> : ICriticalNotifyCompletion
    {
        private readonly Optional<TaskAwaiter<T>> _awaiter;

        internal OptionalTaskAwaiter(TaskAwaiter<T> awaiter)
        {
            _awaiter = awaiter;
        }

        public bool IsCompleted => !_awaiter.HasValue || _awaiter.Value.IsCompleted;

        public Optional<T> GetResult() => _awaiter.HasValue ? Optional.Of(_awaiter.Value.GetResult()) : Optional<T>.None;

        public void OnCompleted(Action continuation)
        {
            if (_awaiter.HasValue) {
                _awaiter.Value.OnCompleted(continuation);
            }
            else {
                continuation();
            }
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            if (_awaiter.HasValue) {
                _awaiter.Value.UnsafeOnCompleted(continuation);
            }
            else {
                continuation();
            }
        }
    }

#if !NETSTANDARD2_0

    public readonly struct OptionalValueTaskAwaiter : ICriticalNotifyCompletion
    {
        private readonly Optional<ValueTaskAwaiter> _awaiter;

        internal OptionalValueTaskAwaiter(ValueTaskAwaiter taskAwaiter)
        {
            _awaiter = taskAwaiter;
        }

        public bool IsCompleted => !_awaiter.HasValue || _awaiter.Value.IsCompleted;

        public void GetResult()
        {
            if (_awaiter.HasValue)
                _awaiter.Value.GetResult();
        }

        public void OnCompleted(Action continuation)
        {
            if (_awaiter.HasValue) {
                _awaiter.Value.OnCompleted(continuation);
            }
            else {
                continuation();
            }
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            if (_awaiter.HasValue) {
                _awaiter.Value.UnsafeOnCompleted(continuation);
            }
            else {
                continuation();
            }
        }
    }

    public readonly struct OptionalValueTaskAwaiter<T> : ICriticalNotifyCompletion
    {
        private readonly Optional<ValueTaskAwaiter<T>> _awaiter;

        internal OptionalValueTaskAwaiter(ValueTaskAwaiter<T> awaiter)
        {
            _awaiter = awaiter;
        }

        public bool IsCompleted => !_awaiter.HasValue || _awaiter.Value.IsCompleted;

        public Optional<T> GetResult() => _awaiter.HasValue ? Optional.Of(_awaiter.Value.GetResult()) : Optional<T>.None;

        public void OnCompleted(Action continuation)
        {
            if (_awaiter.HasValue) {
                _awaiter.Value.OnCompleted(continuation);
            }
            else {
                continuation();
            }
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            if (_awaiter.HasValue) {
                _awaiter.Value.UnsafeOnCompleted(continuation);
            }
            else {
                continuation();
            }
        }
    }

#endif
}

#endif
