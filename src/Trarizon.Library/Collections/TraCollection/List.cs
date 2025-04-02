using CommunityToolkit.HighPerformance;
using System.Runtime.InteropServices;

namespace Trarizon.Library.Collections;
public static partial class TraCollection
{
#if NETSTANDARD

    /// <remarks>
    /// As CollectionsMarshal doesnt exists on .NET Standard 2.0, this use a very tricky way
    /// to get the underlying array. Actually I'm not sure if this works correctly in all runtime...
    /// (at leat it works on Unity
    /// </remarks>
    public static Span<T> AsSpan<T>(this List<T> list) 
        => Utils<T>.GetUnderlyingArray(list).AsSpan(..list.Count);

#endif

    public static T[] GetUnderlyingArray<T>(List<T> list)
        => Utils<T>.GetUnderlyingArray(list);

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
        Utils<T>.GetVersion(list)++;
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
        Utils<T>.GetVersion(list)++;
    }

    public static void MoveTo<T>(this List<T> list, Range fromRange, Index toIndex)
    {
        var (fromOfs, len) = fromRange.GetOffsetAndLength(list.Count);
        var toOfs = toIndex.GetOffset(list.Count);
        list.MoveTo(fromOfs, toOfs, len);
    }
}
