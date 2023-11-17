namespace Trarizon.Library.Collections.Extensions;
public static partial class ArrayExtensions
{
    public static void Fill<T>(this T[] array, T item)
        => array.AsSpan().Fill(item);

    public static void SortStably<T>(this T[] values, Comparison<T>? comparison = null)
        => values.AsSpan().SortStably(comparison);
}
