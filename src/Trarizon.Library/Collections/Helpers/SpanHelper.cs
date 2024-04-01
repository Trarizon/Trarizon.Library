using System.Buffers;
using System.Runtime.InteropServices;
using Trarizon.Library.Collections.StackAlloc;
using Trarizon.Library.Helpers;

namespace Trarizon.Library.Collections.Helpers;
public static partial class SpanHelper
{
    #region OffsetOf

    /// <summary>
    /// Get index of item by reference substraction.
    /// No out-of-range check, make sure <paramref name="item"/> in <paramref name="span"/>
    /// </summary>
    /// <returns>Index of <paramref name="item"/></returns>
    public static int OffsetOf<T>(this ReadOnlySpan<T> span, ref readonly T item)
        => (int)UnsafeHelper.Offset(in MemoryMarshal.GetReference(span), in item);

    /// <summary>
    /// Get index of item by reference substraction.
    /// No out-of-range check, make sure <paramref name="item"/> in <paramref name="span"/>
    /// </summary>
    /// <returns>Index of <paramref name="item"/></returns>
    public static int OffsetOf<T>(this Span<T> span, ref readonly T item)
        => (int)UnsafeHelper.Offset(in MemoryMarshal.GetReference(span), in item);

    /// <summary>
    /// Get index of the first element in <paramref name="subSpan"/> by reference substraction.
    /// No out-of-range check, make sure <paramref name="subSpan"/> in <paramref name="span"/>
    /// </summary>
    /// <returns>Index of first element in <paramref name="subSpan"/></returns>
    public static int OffsetOf<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> subSpan)
        => (int)UnsafeHelper.Offset(in MemoryMarshal.GetReference(span), in MemoryMarshal.GetReference(subSpan));

    /// <summary>
    /// Get index of the first element in <paramref name="subSpan"/> by reference substraction.
    /// No out-of-range check, make sure <paramref name="subSpan"/> in <paramref name="span"/>
    /// </summary>
    /// <returns>Index of first element in <paramref name="subSpan"/></returns>
    public static int OffsetOf<T>(this Span<T> span, ReadOnlySpan<T> subSpan)
        => (int)UnsafeHelper.Offset(in MemoryMarshal.GetReference(span), in MemoryMarshal.GetReference(subSpan));

    #endregion

    #region IndexOf from specific startIndex

    public static int IndexOf<T>(this Span<T> span, T value, int startIndex) where T : IEquatable<T>?
        => span[startIndex..].IndexOf(value) + startIndex;

    public static int IndexOf<T>(this ReadOnlySpan<T> span, T value, int startIndex) where T : IEquatable<T>?
        => span[startIndex..].IndexOf(value) + startIndex;

    #endregion

    #region SortStably

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

    #endregion

    #region Reverse

    public static ReversedSpan<T> ToReversedSpan<T>(this Span<T> span) => new(span);

    public static ReversedReadOnlySpan<T> ToReversedSpan<T>(this ReadOnlySpan<T> span) => new(span);

    #endregion
}
