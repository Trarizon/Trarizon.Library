using System.Runtime.InteropServices;
using Trarizon.Library.Helpers;

namespace Trarizon.Library.Collections.Helpers;
partial class ArrayHelper
{
    /// <summary>
    /// Get index of item by reference substraction.
    /// No out-of-range check
    /// </summary>
    /// <returns>Index of <paramref name="item"/></returns>
    public static int OffsetOf<T>(this T[] array, ref readonly T item)
        => (int)UnsafeHelper.Offset(in MemoryMarshal.GetArrayDataReference(array), in item);

    /// <summary>
    /// Get index of the first element in <paramref name="span"/> by reference substraction.
    /// No out-of-range check
    /// </summary>
    /// <returns>Index of first element in <paramref name="span"/></returns>
    public static int OffsetOf<T>(this T[] array, ReadOnlySpan<T> span)
        => (int)UnsafeHelper.Offset(in MemoryMarshal.GetArrayDataReference(array), in MemoryMarshal.GetReference(span));
}
