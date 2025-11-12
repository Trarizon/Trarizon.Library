using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Trarizon.Library.Collections.Helpers;

namespace Trarizon.Library.Collections;

public static partial class TraCollection
{
#if NETSTANDARD2_0

    public static Span<T> AsSpan<T>(this T[] array, Range range)
    {
        var (ofs, len) = range.GetOffsetAndLength(array.Length);
        return array.AsSpan(ofs, len);
    }

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

#if NETSTANDARD

    public static int MaxLength => 0X7FFFFFC7;

    public static Span<T> AsSpan<T>(List<T> list)
        => UnsafeAccess<T>.GetItems(list).AsSpan(0, list.Count);

    public static void EnsureCapacity<T>(this List<T> list, int expectedCapacity)
    {
        if (expectedCapacity <= list.Capacity)
            return;
        list.Capacity = ArrayGrowHelper.GetNewLength(list.Capacity, expectedCapacity);
    }

    public static bool TryGetNonEnumeratedCount<T>(this IEnumerable<T> source, out int count)
    {
        if (source is ICollection<T> collection) {
            count = collection.Count;
            return true;
        }
        if (source is ICollection ngcollection) {
            count = ngcollection.Count;
            return true;
        }
        count = 0;
        return false;
    }

#endif
}
