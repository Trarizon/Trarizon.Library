using System.Runtime.CompilerServices;
using Trarizon.Library.Wrappers;

namespace Trarizon.Library.Helpers;
partial class AsyncHelper
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

    public readonly struct NullableValueTaskAwaiter<T> : INotifyCompletion
    {
        private readonly ValueTaskAwaiter<T>? _valueTaskAwaiter;
        internal NullableValueTaskAwaiter(ValueTask<T>? valueTask) => _valueTaskAwaiter = valueTask?.GetAwaiter();
        public bool IsCompleted => _valueTaskAwaiter?.IsCompleted ?? true;
        public void OnCompleted(Action continuation) => _valueTaskAwaiter?.OnCompleted(continuation);
        public Optional<T> GetResult() => _valueTaskAwaiter.HasValue ? _valueTaskAwaiter.Value.GetResult() : Optional.None<T>();
    }
}
