using CommunityToolkit.Diagnostics;
using CommunityToolkit.HighPerformance.Buffers;
using System.Diagnostics;

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

    public static void MoveTo<T>(this Span<T> span, int fromIndex, int toIndex, int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(fromIndex);
        ArgumentOutOfRangeException.ThrowIfNegative(toIndex);

        if (length <= 0)
            return;
        if (fromIndex == toIndex)
            return;

        if (fromIndex > toIndex) {
            ArgumentOutOfRangeException.ThrowIfGreaterThan(fromIndex + length, span.Length);
            Core(span, toIndex, toIndex + length, length, fromIndex - toIndex);
        }
        else {
            ArgumentOutOfRangeException.ThrowIfGreaterThan(toIndex + length, span.Length);
            Core(span, fromIndex, toIndex, toIndex - fromIndex, length);
        }

        static void Core(Span<T> span, int from, int to, int dist, int length)
        {
            Debug.Assert(to - from == dist);
            Debug.Assert(dist > 0);
            Debug.Assert(dist != length);

            if (dist < length) {
                using var bufferOwner = SpanOwner<T>.Allocate(dist);
                var buffer = bufferOwner.Span;
                span.Slice(from + length, dist).CopyTo(buffer);
                span.Slice(from, length).CopyTo(span.Slice(to, length));
                buffer.CopyTo(span.Slice(from, dist));
            }
            else {
                using var bufferOwner = SpanOwner<T>.Allocate(length);
                var buffer = bufferOwner.Span;
                span.Slice(from, length).CopyTo(buffer);
                span.Slice(from + length, dist).CopyTo(span.Slice(from, dist));
                buffer.CopyTo(span.Slice(to, length));
            }
        }
    }

    public static void MoveTo<T>(this Span<T> span, Range fromRange, Index toIndex)
    {
        var (ofs, len) = fromRange.GetOffsetAndLength(span.Length);
        var to = toIndex.GetOffset(span.Length);
        span.MoveTo(ofs, to, len);
    }

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
