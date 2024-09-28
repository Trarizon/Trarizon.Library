using System.Runtime.InteropServices;

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
        => CollectionsMarshal.AsSpan(list).MoveTo(fromIndex.GetOffset(list.Count), toIndex.GetOffset(list.Count));
}
