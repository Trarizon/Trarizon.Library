using System.Diagnostics;
using System.Runtime.InteropServices;
using Trarizon.Library.Collections.Helpers;

namespace Trarizon.Library.Collections;
public static partial class TraCollection
{
    // This may conflict to CommunityToolkit.HighPerformance, so we don't public it
    // But we public a non-extension method for .NET Standard
    /// <remarks>
    /// As CollectionsMarshal doesnt exists on .NET Standard 2.0, this use a very tricky way
    /// to get the underlying array. Actually I'm not sure if this works correctly in all runtime...
    /// (at least it works on Unity
    /// </remarks>
    internal static Span<T> AsListSpan<T>(this List<T> list)
    {
#if NETSTANDARD
        return Utils<T>.GetUnderlyingArray(list).AsSpan(..list.Count);
#else
        return CollectionsMarshal.AsSpan(list);
#endif
    }

#if NETSTANDARD

    public static Span<T> AsSpan<T>(List<T> list)
        => AsListSpan(list);

    public static void EnsureCapacity<T>(this List<T> list, int expectedCapacity)
    {
        if (expectedCapacity <= list.Capacity)
            return;
        list.Capacity = ArrayGrowHelper.GetNewLength(list.Capacity, expectedCapacity);
    }

#endif

    public static ref T AtRef<T>(this List<T> list, int index)
        => ref list.AsListSpan()[index];

    public static T[] GetUnderlyingArray<T>(List<T> list)
        => Utils<T>.GetUnderlyingArray(list);

    public static void AddRange<T>(this List<T> list, ReadOnlySpan<T> span)
    {
#if NET8_0_OR_GREATER
        var oldCount = list.Count;
        CollectionsMarshal.SetCount(list, oldCount + span.Length);
        span.CopyTo(list.AsListSpan().Slice(oldCount, span.Length));
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
        list.AsListSpan().MoveTo(fromIndex, toIndex);
        IncrementVersion(list);
    }

    public static void MoveTo<T>(this List<T> list, Index fromIndex, Index toIndex)
    {
        var count = list.Count;
        list.MoveTo(fromIndex.GetOffset(count), toIndex.GetOffset(count));
    }

    public static void MoveTo<T>(this List<T> list, int fromIndex, int toIndex, int moveCount)
    {
        list.AsListSpan().MoveTo(fromIndex, toIndex, moveCount);
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
        Utils<T>.GetVersion(list)++;
#elif NETSTANDARD
        // If list is empty, and count no change, normally there's no operation requires updating version
        Debug.Assert(list.Count > 0);
        list[0] = MemoryMarshal.GetReference(list.AsListSpan());
#else
        CollectionsMarshal.SetCount(list, list.Count);
#endif
    }

    #endregion
}
