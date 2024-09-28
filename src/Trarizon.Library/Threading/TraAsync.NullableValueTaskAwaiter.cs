using System.Runtime.CompilerServices;

namespace Trarizon.Library.Threading;
partial class TraAsync
{
    public readonly struct NullableValueTaskAwaiter<T> : INotifyCompletion, ICriticalNotifyCompletion
    {
        private readonly ValueTaskAwaiter<T>? _valueTaskAwaiter;

        internal NullableValueTaskAwaiter(ValueTask<T>? valueTask) => _valueTaskAwaiter = valueTask?.GetAwaiter();

        public bool IsCompleted => _valueTaskAwaiter?.IsCompleted ?? true;
        public void OnCompleted(Action continuation) => _valueTaskAwaiter?.OnCompleted(continuation);
        public T? GetResult() => _valueTaskAwaiter.HasValue ? Nullable.GetValueRefOrDefaultRef(in _valueTaskAwaiter).GetResult() : default;
        public void UnsafeOnCompleted(Action continuation) => _valueTaskAwaiter?.UnsafeOnCompleted(continuation);
    }
}
