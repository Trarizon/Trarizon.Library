using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Trarizon.Library.Collections;
partial class TraSpan
{
    public static bool ContainsByComparer<T>(this ReadOnlySpan<T> span, T item)
    {
        if (typeof(T).IsValueType) {
            ref readonly var newref = ref Unsafe.As<T, TraComparison.DefaultComparerEquatable<T>>(ref MemoryMarshal.GetReference(span));
            return MemoryMarshal.CreateReadOnlySpan(in newref, span.Length).Contains(new TraComparison.DefaultComparerEquatable<T>(item));
        }
        else {
            return ContainsByComparer(span, item, EqualityComparer<T>.Default);
        }
    }

    public static bool ContainsByComparer<T>(this Span<T> span, T item)
        => ContainsByComparer((ReadOnlySpan<T>)span, item);

    public static bool ContainsByComparer<T, TComparer>(this ReadOnlySpan<T> span, T item, TComparer comparer) where TComparer : IEqualityComparer<T>
    {
        foreach (var v in span) {
            if (comparer.Equals(v, item))
                return true;
        }
        return false;
    }

    public static bool ContainsByComparer<T, TComparer>(this Span<T> span, T item, TComparer comparer) where TComparer : IEqualityComparer<T>
        => ContainsByComparer((ReadOnlySpan<T>)span, item, comparer);
}
