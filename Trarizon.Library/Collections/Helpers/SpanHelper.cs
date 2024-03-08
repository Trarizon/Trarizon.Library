using System.Runtime.InteropServices;
using Trarizon.Library.Collections.Helpers.Utilities;
using Trarizon.Library.Collections.StackAlloc;

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
        => UnsafeUtil.SubstractRef(in item, in MemoryMarshal.GetReference(span));

    /// <summary>
    /// Get index of item by reference substraction.
    /// No out-of-range check, make sure <paramref name="item"/> in <paramref name="span"/>
    /// </summary>
    /// <returns>Index of <paramref name="item"/></returns>
    public static int OffsetOf<T>(this Span<T> span, ref readonly T item)
        => UnsafeUtil.SubstractRef(in item, in MemoryMarshal.GetReference(span));

    /// <summary>
    /// Get index of the first element in <paramref name="subSpan"/> by reference substraction.
    /// No out-of-range check, make sure <paramref name="subSpan"/> in <paramref name="span"/>
    /// </summary>
    /// <returns>Index of first element in <paramref name="subSpan"/></returns>
    public static int OffsetOf<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> subSpan)
        => UnsafeUtil.SubstractRef(in MemoryMarshal.GetReference(subSpan), in MemoryMarshal.GetReference(span));

    /// <summary>
    /// Get index of the first element in <paramref name="subSpan"/> by reference substraction.
    /// No out-of-range check, make sure <paramref name="subSpan"/> in <paramref name="span"/>
    /// </summary>
    /// <returns>Index of first element in <paramref name="subSpan"/></returns>
    public static int OffsetOf<T>(this Span<T> span, ReadOnlySpan<T> subSpan)
        => UnsafeUtil.SubstractRef(in MemoryMarshal.GetReference(subSpan), in MemoryMarshal.GetReference(span));

    #endregion

    #region IndexOf from specific startIndex

    public static int IndexOf<T>(this Span<T> span, T value, int startIndex) where T : IEquatable<T>?
        => span[startIndex..].IndexOf(value) + startIndex;

    public static int IndexOf<T>(this ReadOnlySpan<T> span, T value, int startIndex) where T : IEquatable<T>?
        => span[startIndex..].IndexOf(value) + startIndex;

    #endregion

    #region SortStably

    public static void SortStably<T>(this Span<T> span, Comparison<T>? comparison = null)
        => span.SortStably(comparison is null ? null : new StableSortComparer<T>(comparison));

    public static void SortStably<T>(this Span<T> span, StableSortComparer<T>? comparer = null)
    {
        Span<(int, T)> keys = new (int, T)[span.Length];
        for (int i = 0; i < span.Length; i++)
            keys[i] = (i, span[i]);
        keys.Sort(span, comparer ?? StableSortComparer<T>.Default);
    }

    #endregion

    #region Reverse

    public static ReversedSpan<T> ToReversedSpan<T>(this Span<T> span) => new(span);

    public static ReversedReadOnlySpan<T> ToReversedSpan<T>(this ReadOnlySpan<T> span) => new(span);

    #endregion
}
