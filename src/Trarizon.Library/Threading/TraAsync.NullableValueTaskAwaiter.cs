using System.Runtime.CompilerServices;

namespace Trarizon.Library.Threading;
partial class TraAsync
{
    /// <remarks>
    /// Provide <see langword="await"/> feature support for nullable <see cref="ValueTask"/>
    /// </remarks>
    public static ValueTaskAwaiter GetAwaiter(this ValueTask? task)
#if NETSTANDARD2_0
        => task.GetValueOrDefault().GetAwaiter();
#else
        => (task ?? ValueTask.CompletedTask).GetAwaiter();
#endif

    /// <remarks>
    /// Provide <see langword="await"/> feature support for nullable <see cref="ValueTask"/>
    /// </remarks>
    public static NullableValueTaskAwaiter<T> GetAwaiter<T>(this ValueTask<T>? task) => new(task);

    public readonly struct NullableValueTaskAwaiter<T> : INotifyCompletion, ICriticalNotifyCompletion
    {
        private readonly ValueTaskAwaiter<T>? _valueTaskAwaiter;

        internal NullableValueTaskAwaiter(ValueTask<T>? valueTask) => _valueTaskAwaiter = valueTask?.GetAwaiter();

        public bool IsCompleted => _valueTaskAwaiter?.IsCompleted ?? true;
        public void OnCompleted(Action continuation) => _valueTaskAwaiter?.OnCompleted(continuation);
        public T? GetResult() => _valueTaskAwaiter.HasValue ? _valueTaskAwaiter.GetValueOrDefault().GetResult() : default;
        public void UnsafeOnCompleted(Action continuation) => _valueTaskAwaiter?.UnsafeOnCompleted(continuation);
    }
}
