using System.Diagnostics;
using System.Runtime.InteropServices;
using Trarizon.Library.Collections.Helpers;

namespace Trarizon.Library.Collections;
public static partial class TraCollection
{


    public static ref T AtRef<T>(this List<T> list, int index)
        => ref list.AsSpan()[index];

    public static T[] GetUnderlyingArray<T>(List<T> list)
        => UnsafeAccess<T>.GetItems(list);

    public static void ReplaceAll<T>(this List<T> list, ReadOnlySpan<T> span)
    {
#if NET8_0_OR_GREATER
        CollectionsMarshal.SetCount(list, span.Length);
        span.CopyTo(CollectionsMarshal.AsSpan(list));
#else
        list.Clear();
        list.AddRange(span);
#endif
    }

    public static void AddRange<T>(this List<T> list, ReadOnlySpan<T> span)
    {
#if NET8_0_OR_GREATER
        var oldCount = list.Count;
        CollectionsMarshal.SetCount(list, oldCount + span.Length);
        span.CopyTo(list.AsSpan().Slice(oldCount, span.Length));
#else
        list.EnsureCapacity(list.Count + span.Length);
        foreach (var item in span) {
            list.Add(item);
        }
#endif
    }

    public static void RemoveAt<T>(this List<T> list, Index index)
        => list.RemoveAt(index.GetOffset(list.Count));

    public static void RemoveRange<T>(this List<T> list, Range range)
    {
        var (off, len) = range.GetOffsetAndLength(list.Count);
        list.RemoveRange(off, len);
    }

    public static void MoveTo<T>(this List<T> list, int fromIndex, int toIndex)
    {
        list.AsSpan().MoveTo(fromIndex, toIndex);
        IncrementVersion(list);
    }

    public static void MoveTo<T>(this List<T> list, Index fromIndex, Index toIndex)
    {
        var count = list.Count;
        list.MoveTo(fromIndex.GetOffset(count), toIndex.GetOffset(count));
    }

    public static void MoveTo<T>(this List<T> list, int fromIndex, int toIndex, int moveCount)
    {
        list.AsSpan().MoveTo(fromIndex, toIndex, moveCount);
        IncrementVersion(list);
    }

    public static void MoveTo<T>(this List<T> list, Range fromRange, Index toIndex)
    {
        var (fromOfs, len) = fromRange.GetOffsetAndLength(list.Count);
        var toOfs = toIndex.GetOffset(list.Count);
        list.MoveTo(fromOfs, toOfs, len);
    }

    #region Internal

    private static void IncrementVersion<T>(List<T> list)
    {
        // If we modified in Span, the field _version of List<> won't update, so here we manually
        // use SetCount to update _version;
#if NET9_0_OR_GREATER
        UnsafeAccess<T>.GetVersion(list)++;
#elif NETSTANDARD
        // If list is empty, and count no change, normally there's no operation requires updating version
        Debug.Assert(list.Count > 0);
        list[0] = MemoryMarshal.GetReference(list.AsSpan());
#else
        CollectionsMarshal.SetCount(list, list.Count);
#endif
    }

    #endregion
}
