using System.Runtime.CompilerServices;
using Trarizon.Library.CodeAnalysis.MemberAccess;
using Trarizon.Library.Wrappers;

namespace Trarizon.Library.Threading.Helpers;
partial class AsyncHelper
{
    public readonly struct NullableValueTaskAwaiter<T> : INotifyCompletion, ICriticalNotifyCompletion
    {
        private readonly ValueTaskAwaiter<T>? _valueTaskAwaiter;
        
        [FriendAccess(typeof(AsyncHelper))]
        internal NullableValueTaskAwaiter(ValueTask<T>? valueTask) => _valueTaskAwaiter = valueTask?.GetAwaiter();
        
        public bool IsCompleted => _valueTaskAwaiter?.IsCompleted ?? true;
        public void OnCompleted(Action continuation) => _valueTaskAwaiter?.OnCompleted(continuation);
        public Optional<T> GetResult() => _valueTaskAwaiter.HasValue ? _valueTaskAwaiter.Value.GetResult() : Optional.None<T>();
        public void UnsafeOnCompleted(Action continuation) => _valueTaskAwaiter?.UnsafeOnCompleted(continuation);
    }
}
