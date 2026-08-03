using System.Runtime.InteropServices;
using Trarizon.Library.Memory;

namespace Trarizon.Library.Collections;

public static partial class TraCollection
{
    // AddRange

    public static void AddRange<T>(this List<T> list, ReadOnlySpan<T> span)
    {
#if NET8_0_OR_GREATER
        var oldCount = list.Count;
        CollectionsMarshal.SetCount(list, oldCount + span.Length);
        span.CopyTo(CollectionsMarshal.AsSpan(list).Slice(oldCount, span.Length));
#else
        list.EnsureCapacity(list.Count + span.Length);
        foreach (var item in span)
        {
            list.Add(item);
        }
#endif
    }

    // Remove

    public static void RemoveAt<T>(this List<T> list, Index index)
        => list.RemoveAt(index.GetOffset(list.Count));

    public static void RemoveRange<T>(this List<T> list, Range range)
    {
        var (off, len) = range.GetOffsetAndLength(list.Count);
        list.RemoveRange(off, len);
    }

    // Find

    public static T? Find<T, TState>(this List<T> list, TState state, Func<T, TState, bool> predicate)
    {
        var span = CollectionsMarshal.AsSpan(list);

        foreach (var item in span)
        {
            if (predicate(item, state))
                return item;
        }
        return default;
    }

    public static T? FindLast<T, TState>(this List<T> list, TState state, Func<T, TState, bool> predicate)
    {
        var span = CollectionsMarshal.AsSpan(list).AsReversed();
        foreach (var item in span)
        {
            if (predicate(item, state))
                return item;
        }
        return default;
    }

    public static void ForEach<T, TState>(this List<T> list, TState state, Action<T, TState> action)
    {
        var span = CollectionsMarshal.AsSpan(list);
        foreach (var item in span)
        {
            action(item, state);
        }
    }
}