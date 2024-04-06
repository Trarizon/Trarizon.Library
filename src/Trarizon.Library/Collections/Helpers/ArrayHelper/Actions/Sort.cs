namespace Trarizon.Library.Collections.Helpers;
partial class ArrayHelper
{
    public static void SortStably<T>(this T[] values, Comparison<T> comparison)
        => values.AsSpan().SortStably(comparison);

    public static void SortStably<T>(this T[] values, StableSortComparer<T>? comparer = null)
        => values.AsSpan().SortStably(comparer);
}
