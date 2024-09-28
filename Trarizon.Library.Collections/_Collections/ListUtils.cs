using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Trarizon.Library.Collections;
public static class ListUtils
{
    public static void RemoveAt<T>(this List<T> list, Index index)
    {
        list.RemoveAt(index.GetOffset(list.Count));
    }

    public static void RemoveRange<T>(this List<T> list, Range range)
    {
        var (offset, length) = range.GetOffsetAndLength(list.Count);
        list.RemoveRange(offset, length);
    }

    public static void MoveTo<T>(this Span<T> span, int fromIndex, int toIndex)
    {
        if (fromIndex == toIndex)
            return;

        var val = span[fromIndex];

        if (fromIndex > toIndex) {
            var len = toIndex - fromIndex;
            span.Slice(toIndex, len).CopyTo(span.Slice(toIndex + 1, len));
        }
        else {
            var len = toIndex - fromIndex;
            span.Slice(fromIndex + 1, len).CopyTo(span.Slice(fromIndex, len));
        }
        span[toIndex] = val;
    }

    public static void MoveTo<T>(this Span<T> span, Range fromRange, int toIndex)
    {
        var (fromIndex, length) = fromRange.GetOffsetAndLength(span.Length);
        if (length <= 0)
            return;
        if (fromIndex == toIndex)
            return;

        var values = SpanUtils.UnsafeCast<T>(stackalloc byte[Unsafe.SizeOf<T>() * length]);
        span.Slice(fromIndex, length).CopyTo(values);

        if (fromIndex > toIndex) {
            var len = fromIndex - toIndex;
            span.Slice(toIndex, len).CopyTo(span.Slice(toIndex + length, len));
        }
        else {
            var len = toIndex - fromIndex;
            span.Slice(fromIndex + length, len).CopyTo(span.Slice(fromIndex, len));
        }
        values.CopyTo(span.Slice(toIndex, values.Length));
    }

    public static void MoveTo<T>(this List<T> list, Index fromIndex, Index toIndex)
        => CollectionsMarshal.AsSpan(list).MoveTo(fromIndex.GetOffset(list.Count), toIndex.GetOffset(list.Count));

    public static void MoveTo<T>(this int[] array, Index fromIndex, Index toIndex)
        => array.AsSpan().MoveTo(fromIndex.GetOffset(array.Length), toIndex.GetOffset(array.Length));

    public static void Swap<T>(this Span<T> span, int i, int j)
    {
        (span[i], span[j]) = (span[j], span[i]);
    }
}
