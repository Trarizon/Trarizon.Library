using System.Collections.Immutable;

namespace Trarizon.Library.Collections;
public static class TraArray
{
    public static void MoveTo<T>(this T[] array, Index fromIndex, Index toIndex)
        => array.AsSpan().MoveTo(fromIndex.GetOffset(array.Length), toIndex.GetOffset(array.Length));

    public static void SortStably<T>(this T[] array, StableSortComparer<T>? comparer = null)
        => array.AsSpan().SortStably(comparer);

    public static void SortStably<T>(this T[] array, Comparison<T> comparison)
        => array.AsSpan().SortStably(comparison);

    public static ImmutableArray<T> EmptyIfDefault<T>(this ImmutableArray<T> array)
        => array.IsDefault ? [] : array;
}
