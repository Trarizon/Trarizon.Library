using System.Runtime.InteropServices;
using Trarizon.Library.Helpers;

namespace Trarizon.Library.Collections.Helpers;
partial class SpanHelper
{
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
}
