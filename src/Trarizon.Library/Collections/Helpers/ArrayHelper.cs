using System.Runtime.InteropServices;
using Trarizon.Library.Helpers;

namespace Trarizon.Library.Collections.Helpers;
public static partial class ArrayHelper
{
    #region OffsetOf

    /// <summary>
    /// Get index of item by reference substraction.
    /// No out-of-range check
    /// </summary>
    /// <returns>Index of <paramref name="item"/></returns>
    public static int OffsetOf<T>(this T[] array, ref readonly T item)
        => UnsafeHelper.SubstractRef(in item, in MemoryMarshal.GetArrayDataReference(array));

    /// <summary>
    /// Get index of the first element in <paramref name="span"/> by reference substraction.
    /// No out-of-range check
    /// </summary>
    /// <returns>Index of first element in <paramref name="span"/></returns>
    public static int OffsetOf<T>(this T[] array, ReadOnlySpan<T> span)
        => UnsafeHelper.SubstractRef(in MemoryMarshal.GetReference(span), in MemoryMarshal.GetArrayDataReference(array));

    #endregion

    public static void Fill<T>(this T[] array, T item)
        => array.AsSpan().Fill(item);

    #region SortStably

    public static void SortStably<T>(this T[] values, Comparison<T> comparison)
        => values.AsSpan().SortStably(comparison);

    public static void SortStably<T>(this T[] values, StableSortComparer<T>? comparer = null)
        => values.AsSpan().SortStably(comparer);

    #endregion

    public static Span<T> AsSpan<T>(this T[,] values, int row)
        => MemoryMarshal.CreateSpan(ref values[row, 0], values.GetLength(1));

    public static ReadOnlySpan<T> AsReadOnlySpan<T>(this T[,] values, int row)
        => MemoryMarshal.CreateReadOnlySpan(ref values[row, 0], values.GetLength(1));
}
