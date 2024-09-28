using CommunityToolkit.Diagnostics;
using CommunityToolkit.HighPerformance.Buffers;

namespace Trarizon.Library.Collections;
partial class TraSpan
{
    public static void MoveTo<T>(this Span<T> span, int fromIndex, int toIndex)
    {
        Guard.IsInRangeFor(fromIndex, span);
        Guard.IsInRangeFor(toIndex, span);

        if (fromIndex == toIndex)
            return;

        var val = span[fromIndex];

        if (fromIndex > toIndex) {
            var len = fromIndex - toIndex;
            span.Slice(toIndex, len).CopyTo(span.Slice(toIndex + 1, len));
        }
        else {
            var len = toIndex - fromIndex;
            span.Slice(fromIndex + 1, len).CopyTo(span.Slice(fromIndex, len));
        }

        span[toIndex] = val;
    }

    public static void MoveTo<T>(this Span<T> span, Index fromIndex, Index toIndex)
        => span.MoveTo(fromIndex.GetOffset(span.Length), toIndex.GetOffset(span.Length));

    public static void SortStably<T>(this Span<T> span, StableSortComparer<T>? comparer = null)
    {
        using var spanOwner = SpanOwner<(int, T)>.Allocate(span.Length);
        var keys = spanOwner.Span;
        for (int i = 0; i < span.Length; i++)
            keys.Sort(span, comparer ?? StableSortComparer<T>.Default);
    }

    public static void SortStably<T>(this Span<T> span, Comparison<T> comparison)
        => span.SortStably(StableSortComparer<T>.Create(comparison));
}
