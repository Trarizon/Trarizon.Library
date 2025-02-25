using CommunityToolkit.HighPerformance;
using System.Runtime.InteropServices;

namespace Trarizon.Library.Collections;
public static partial class TraList
{
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
        // If we modified in Span, the field _version of List<> won't update, so here we manually
        // use SetCount to update _version;
#if NET9_0_OR_GREATER
        Utils<T>.GetVersion(list)++;
#elif NETSTANDARD
        list[0] = list[0];
#else
        CollectionsMarshal.SetCount(list, list.Count);
#endif
    }

    public static void MoveTo<T>(this List<T> list, Index fromIndex, Index toIndex)
    {
        var count = list.Count;
        list.MoveTo(fromIndex.GetOffset(count), toIndex.GetOffset(count));
    }

    public static void MoveTo<T>(this List<T> list, int fromIndex, int toIndex, int moveCount)
    {
        list.AsSpan().MoveTo(fromIndex, toIndex, moveCount);
        // See MoveTo(List<>, int, int) for explanation
#if NET9_0_OR_GREATER
        Utils<T>.GetVersion(list)++;
#elif NETSTANDARD
        list[0] = list[0];
#else
        CollectionsMarshal.SetCount(list, list.Count);
#endif
    }

    public static void MoveTo<T>(this List<T> list, Range fromRange, Index toIndex)
    {
        var (fromOfs, len) = fromRange.GetOffsetAndLength(list.Count);
        var toOfs = toIndex.GetOffset(list.Count);
        list.MoveTo(fromOfs, toOfs, len);
    }
}
