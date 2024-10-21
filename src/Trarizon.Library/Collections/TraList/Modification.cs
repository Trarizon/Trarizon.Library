using CommunityToolkit.HighPerformance;
using System.Diagnostics;

namespace Trarizon.Library.Collections;
partial class TraList
{
    public static void RemoveAt<T>(this List<T> list, Index index)
        => list.RemoveAt(index.GetOffset(list.Count));

    public static void RemoveRange<T>(this List<T> list, Range range)
    {
        var (off, len) = range.GetOffsetAndLength(list.Count);
        list.RemoveRange(off, len);
    }

    public static void MoveTo<T>(this List<T> list, Index fromIndex, Index toIndex)
    {
        var fromOfs = fromIndex.GetOffset(list.Count);
        var toOfs = toIndex.GetOffset(list.Count);
        var item = list[fromOfs];
        list.AsSpan().MoveTo(fromOfs, toOfs);
        // If we modified in Span, the field _version of List<> won't update, so here we manually
        // do an assignment to update the field _version;
        Debug.Assert(EqualityComparer<T>.Default.Equals(list[toOfs], item));
#if NET9_0_OR_GREATER
        Utils<T>.GetVersion(list)++;
#else
        list[toOfs] = item;
#endif
    }

    public static void MoveTo<T>(this List<T> list, int fromIndex, int toIndex, int moveCount)
    {
        var item = list[fromIndex];
        list.AsSpan().MoveTo(fromIndex, toIndex, moveCount);
        // See MoveTo(List<>, Index, Index) for explanation
        Debug.Assert(EqualityComparer<T>.Default.Equals(list[toIndex], item));
#if NET9_0_OR_GREATER
        Utils<T>.GetVersion(list)++;
#else
        list[toIndex] = item;
#endif
    }

    public static void MoveTo<T>(this List<T> list, Range fromRange, Index toIndex)
    {
        var (fromOfs, len) = fromRange.GetOffsetAndLength(list.Count);
        var toOfs = toIndex.GetOffset(list.Count);
        list.MoveTo(fromOfs, toOfs, len);
    }
}
