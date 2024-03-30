using System.Runtime.InteropServices;

namespace Trarizon.Library.Collections.Helpers;
public static partial class ListHelper
{
    public static void Fill<T>(this List<T> list, T item)
        => CollectionsMarshal.AsSpan(list).Fill(item);

    #region SortStably

    public static void SortStably<T>(this List<T> list, Comparison<T> comparison)
        => CollectionsMarshal.AsSpan(list).SortStably(comparison);

    public static void SortStably<T>(this List<T> list, StableSortComparer<T>? comparer = null)
        => CollectionsMarshal.AsSpan(list).SortStably(comparer);

    #endregion

    public static ref T AtRef<T>(this List<T> list, int index)
        => ref CollectionsMarshal.AsSpan(list)[index];
}
