using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Trarizon.Library.Collections.Extensions.Helper;

namespace Trarizon.Library.Collections.Extensions;
public static partial class SpanExtensions
{
    #region OffsetOf

    /// <summary>
    /// Get index of item by reference substraction.
    /// No out-of-range check, make sure <paramref name="item"/> in <paramref name="span"/>
    /// </summary>
    /// <returns>Index of <paramref name="item"/></returns>
    public static int OffsetOf<T>(this ReadOnlySpan<T> span, ref readonly T item)
        => Substract(in item, ref MemoryMarshal.GetReference(span));

    /// <summary>
    /// Get index of item by reference substraction.
    /// No out-of-range check, make sure <paramref name="item"/> in <paramref name="span"/>
    /// </summary>
    /// <returns>Index of <paramref name="item"/></returns>
    public static int OffsetOf<T>(this Span<T> span, ref readonly T item)
        => Substract(in item, ref MemoryMarshal.GetReference(span));

    /// <summary>
    /// Get index of the first element in <paramref name="subSpan"/> by reference substraction.
    /// No out-of-range check, make sure <paramref name="subSpan"/> in <paramref name="span"/>
    /// </summary>
    /// <returns>Index of first element in <paramref name="subSpan"/></returns>
    public static int OffsetOf<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> subSpan)
        => Substract(in MemoryMarshal.GetReference(subSpan), ref MemoryMarshal.GetReference(span));

    /// <summary>
    /// Get index of the first element in <paramref name="subSpan"/> by reference substraction.
    /// No out-of-range check, make sure <paramref name="subSpan"/> in <paramref name="span"/>
    /// </summary>
    /// <returns>Index of first element in <paramref name="subSpan"/></returns>
    public static int OffsetOf<T>(this Span<T> span, ReadOnlySpan<T> subSpan)
        => Substract(in MemoryMarshal.GetReference(subSpan), ref MemoryMarshal.GetReference(span));

    private static unsafe int Substract<T>(ref readonly T left, ref readonly T right)
    {
#pragma warning disable CS8500
        fixed (T* rPtr = &right, lPtr = &left) {
            var res = (int)((nuint)lPtr - (nuint)rPtr) / sizeof(T);
#if NET8_0_OR_GREATER
            Debug.Assert(Unsafe.AreSame(in Unsafe.Subtract(ref Unsafe.AsRef(in left), res), in right));
#endif
            return res;
        }
#pragma warning restore CS8500
    }

    #endregion

    public static int IndexOf<T>(this Span<T> span, T value, int startIndex) where T : IEquatable<T>?
        => span[startIndex..].IndexOf(value) + startIndex;
    public static int IndexOf<T>(this ReadOnlySpan<T> span, T value, int startIndex) where T : IEquatable<T>?
        => span[startIndex..].IndexOf(value) + startIndex;

    public static ReversedSpan<T> Reverse<T>(this Span<T> span) => new(span);
    public static ReversedReadOnlySpan<T> Reverse<T>(this ReadOnlySpan<T> span) => new(span);

#if NET8_0_OR_GREATER

    public static void SortStably<T>(this Span<T> span, Comparison<T>? comparison = null)
    {
        Span<(int, T)> keys = new (int, T)[span.Length];
        for (int i = 0; i < span.Length; i++)
            keys[i] = (i, span[i]);
        keys.Sort(span, comparison is null
            ? StableSortComparer<T>.Default
            : new StableSortComparer<T>(comparison));
    }

#endif
}
