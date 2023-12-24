using System.Runtime.InteropServices;

namespace Trarizon.Library.Collections.Extensions;
public static partial class ArrayExtensions
{
    public static void Fill<T>(this T[] array, T item)
        => array.AsSpan().Fill(item);

#if NET8_0_OR_GREATER

    public static void SortStably<T>(this T[] values, Comparison<T>? comparison = null)
        => values.AsSpan().SortStably(comparison);

    public static Span<T> AsSpan<T>(this T[,] values, int row) 
        => MemoryMarshal.CreateSpan(ref values[row, 0], values.GetLength(1));

    public static ReadOnlySpan<T> AsReadOnlySpan<T>(this T[,] values, int row) 
        => MemoryMarshal.CreateReadOnlySpan(ref values[row, 0], values.GetLength(1));

#endif
}
