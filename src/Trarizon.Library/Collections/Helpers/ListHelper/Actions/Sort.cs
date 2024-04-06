using System.Runtime.InteropServices;

namespace Trarizon.Library.Collections.Helpers;
partial class ListHelper
{
    public static void SortStably<T>(this List<T> list, Comparison<T> comparison)
        => CollectionsMarshal.AsSpan(list).SortStably(comparison);

    public static void SortStably<T>(this List<T> list, StableSortComparer<T>? comparer = null)
        => CollectionsMarshal.AsSpan(list).SortStably(comparer);
}
