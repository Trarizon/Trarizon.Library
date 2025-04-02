using CommunityToolkit.Diagnostics;
using Trarizon.Library.Numerics;

namespace Trarizon.Library.Collections;
public static partial class TraSpan
{
    public static int LinearSearch<T, TComparer>(this Span<T> span, T item, TComparer comparer) where TComparer : IComparer<T>
        => LinearSearch((ReadOnlySpan<T>)span, new TraComparison.ComparerComparable<T, TComparer>(item, comparer));

    public static int LinearSearch<T, TComparer>(this ReadOnlySpan<T> span, T item, TComparer comparer) where TComparer : IComparer<T>
        => LinearSearch(span, new TraComparison.ComparerComparable<T, TComparer>(item, comparer));

    public static int LinearSearch<T, TComparable>(this Span<T> span, TComparable item) where TComparable : IComparable<T>
        => TraAlgorithm.LinearSearch<T, TComparable>(span, item);

    public static int LinearSearch<T, TComparable>(this ReadOnlySpan<T> span, TComparable item) where TComparable : IComparable<T>
        => TraAlgorithm.LinearSearch(span, item);

    public static int LinearSearchFromEnd<T, TComparer>(this Span<T> span, T item, TComparer comparer) where TComparer : IComparer<T>
        => LinearSearchFromEnd((ReadOnlySpan<T>)span, new TraComparison.ComparerComparable<T, TComparer>(item, comparer));

    public static int LinearSearchFromEnd<T, TComparer>(this ReadOnlySpan<T> span, T item, TComparer comparer) where TComparer : IComparer<T>
        => LinearSearchFromEnd(span, new TraComparison.ComparerComparable<T, TComparer>(item, comparer));

    public static int LinearSearchFromEnd<T, TComparable>(this Span<T> span, TComparable item) where TComparable : IComparable<T>
        => TraAlgorithm.LinearSearchFromEnd<T, TComparable>(span, item);

    public static int LinearSearchFromEnd<T, TComparable>(this ReadOnlySpan<T> span, TComparable item) where TComparable : IComparable<T>
        => TraAlgorithm.LinearSearchFromEnd(span, item);

    internal static int BinarySearchRangePriority<T, TComparable>(ReadOnlySpan<T> span, Range priorRange, TComparable item) where TComparable : IComparable<T>
    {
        var (start, end) = priorRange.GetCheckedStartAndEndOffset(span.Length);

        var cmpl = item.CompareTo(span[start]);
        if (cmpl > 0) {
            if (end < span.Length) {
                var cmpr = item.CompareTo(span[end]);
                if (cmpr < 0) {
                    var s = start + 1;
                    var i = span[s..end].BinarySearch(item);
                    return i >= 0 ? i + s : i - s;
                }
                else if (cmpr == 0)
                    return end;
                else {
                    var e = end + 1;
                    var i = span[e..].BinarySearch(item);
                    return i >= 0 ? i + e : i - e;
                }
            }
            else {
                var s = start + 1;
                var i = span[s..end].BinarySearch(item);
                return i >= 0 ? i + s : i - s;
            }
        }
        else if (cmpl == 0)
            return start;
        else
            return span[..start].BinarySearch(item);
    }

    internal static int LinearSearchFromNear<T, TComparable>(ReadOnlySpan<T> span, int nearIndex, TComparable item) where TComparable : IComparable<T>
    {
        Guard.IsInRange(nearIndex, 0, span.Length);
        var cmp = item.CompareTo(span[nearIndex]);
        if (cmp == 0)
            return nearIndex;
        else if (cmp < 0)
            return span[..nearIndex].LinearSearchFromEnd(item);
        else {
            var i = span[nearIndex..].LinearSearch(item);
            return i >= 0 ? i + nearIndex : i - nearIndex;
        }
    }
}
