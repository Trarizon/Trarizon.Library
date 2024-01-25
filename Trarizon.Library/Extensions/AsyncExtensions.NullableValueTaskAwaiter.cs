using System.Runtime.CompilerServices;

namespace Trarizon.Library.Extensions;
partial class AsyncExtensions
{
    public readonly struct NullableValueTaskAwaiter : INotifyCompletion, ICriticalNotifyCompletion
    {
        private readonly ValueTaskAwaiter? _valueTaskAwaiter;
        internal NullableValueTaskAwaiter(ValueTask? valueTask) => _valueTaskAwaiter = valueTask?.GetAwaiter();
        public bool IsCompleted => _valueTaskAwaiter?.IsCompleted ?? true;
        public void OnCompleted(Action continuation) => _valueTaskAwaiter?.OnCompleted(continuation);
        public void GetResult() => _valueTaskAwaiter?.GetResult();
        public void UnsafeOnCompleted(Action continuation) => _valueTaskAwaiter?.UnsafeOnCompleted(continuation);
    }

#if false
    public readonly struct NullableValueTaskAwaiter<T> : INotifyCompletion
    {
        private readonly ValueTaskAwaiter<T>? _valueTaskAwaiter;
        internal NullableValueTaskAwaiter(ValueTask<T>? valueTask) => _valueTaskAwaiter = valueTask?.GetAwaiter();
        public bool IsCompleted => _valueTaskAwaiter?.IsCompleted ?? true;
        public void OnCompleted(Action continuation) => _valueTaskAwaiter?.OnCompleted(continuation);
        public T? GetResult() => _valueTaskAwaiter.HasValue ? _valueTaskAwaiter.Value.GetResult() : default;
    }
#endif
}
