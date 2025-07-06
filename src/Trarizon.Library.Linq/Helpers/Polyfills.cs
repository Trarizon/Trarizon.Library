using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library.Linq.Helpers;
#if NETSTANDARD2_0

internal static class PfRuntimeHelpers
{
    public static bool IsReferenceOrContainsReferences<T>()
    {
        return !typeof(T).IsPrimitive;
    }
}

internal static class CollectionExtensions
{
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