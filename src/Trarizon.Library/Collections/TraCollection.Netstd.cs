#if NETSTANDARD2_0

using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library.Collections;
public static partial class TraCollection
{
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
}

#endif
