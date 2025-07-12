using System.Diagnostics.CodeAnalysis;
using Trarizon.Library.Collections.StackAlloc;

namespace Trarizon.Library.Collections;
public static partial class TraCollection
{
    public static ReversedSpan<T> AsSpan<T>(Stack<T> stack)
        => Utils<T>.GetUnderlyingArray(stack).AsSpan(..stack.Count).AsReversed();

    #region Polyfills

#if NETSTANDARD2_0

    public static bool TryPop<T>(this Stack<T> stack, [MaybeNullWhen(false)] out T value)
    {
        if (stack.Count == 0) {
            value = default;
            return false;
        }
        value = stack.Pop();
        return true;
    }

    public static bool TryPeek<T>(this Stack<T> stack, [MaybeNullWhen(false)] out T value)
    {
        if (stack.Count == 0) {
            value = default;
            return false;
        }
        value = stack.Peek();
        return true;
    }

    public static bool TryDequeue<T>(this Queue<T> queue, [MaybeNullWhen(false)] out T value)
    {
        if (queue.Count == 0) {
            value = default;
            return false;
        }
        value = queue.Dequeue();
        return true;
    }

    public static bool TryPeek<T>(this Queue<T> queue, [MaybeNullWhen(false)] out T value)
    {
        if (queue.Count == 0) {
            value = default;
            return false;
        }
        value = queue.Peek();
        return true;
    }

#endif

    #endregion
}
