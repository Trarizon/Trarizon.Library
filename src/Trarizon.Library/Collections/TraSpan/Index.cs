using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Trarizon.Library.Collections;
partial class TraSpan
{
    public static int OffsetOf<T>(this Span<T> span, ref readonly T item)
        => ((ReadOnlySpan<T>)span).OffsetOf(in item);

    public static int OffsetOf<T>(this ReadOnlySpan<T> span, ref readonly T item)
        => (int)Unsafe.ByteOffset(in MemoryMarshal.GetReference(span), in item) / Unsafe.SizeOf<T>();

    public static int FindLowerBoundIndex<T>(this Span<T> span, T key) where T : IComparisonOperators<T, T, bool>
        => ((ReadOnlySpan<T>)span).FindLowerBoundIndex(key);

    public static int FindLowerBoundIndex<T>(this ReadOnlySpan<T> span, T key) where T : IComparisonOperators<T, T, bool>
    {
        for (int i = 0; i < span.Length; i++) {
            if (key <= span[i]) {
                return i;
            }
        }
        return span.Length;
    }

    public static int FindUpperBoundIndex<T>(this Span<T> span, T key) where T : IComparisonOperators<T, T, bool>
        => ((ReadOnlySpan<T>)span).FindUpperBoundIndex(key);

    public static int FindUpperBoundIndex<T>(this ReadOnlySpan<T> span, T key) where T : IComparisonOperators<T, T, bool>
    {
        for (int i = 0; i < span.Length; i++) {
            if (key < span[i]) {
                return i;
            }
        }
        return span.Length;
    }
}
