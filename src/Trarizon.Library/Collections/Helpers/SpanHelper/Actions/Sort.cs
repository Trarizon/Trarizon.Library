using System.Buffers;

namespace Trarizon.Library.Collections.Helpers;
partial class SpanHelper
{
    public static void SortStably<T>(this Span<T> span, Comparison<T> comparison)
        => span.SortStably(new StableSortComparer<T>(comparison));

    public static void SortStably<T>(this Span<T> span, StableSortComparer<T>? comparer = null)
    {
        var keys = ArrayPool<(int, T)>.Shared.Rent(span.Length);
        try {
            for (int i = 0; i < span.Length; i++)
                keys[i] = (i, span[i]);
            keys.AsSpan(0, span.Length).Sort(span, comparer ?? StableSortComparer<T>.Default);
        } finally {
            ArrayPool<(int, T)>.Shared.Return(keys);
        }
    }
}
