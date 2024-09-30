using Trarizon.Library.Collections.Generic;

namespace Trarizon.Library.Collections;
partial class TraList
{
    public static Memory<T> AsMemory<T>(this List<T> list) 
        => Utils<T>.GetUnderlyingArray(list).AsMemory(list.Count);

    public static Lookup<T> GetLookup<T>(this List<T> list, IEqualityComparer<T>? comparer = null)
        => new(list, comparer ?? EqualityComparer<T>.Default);

    public static KeyedLookup<T, TKey> GetKeyedLookup<T, TKey>(this List<T> list, IKeyedEqualityComparer<T, TKey> comparer)
        => new(list, comparer);

    public static KeyedLookup<(TKey, TValue), TKey> GetKeyedLookup<TKey, TValue>(this List<(TKey, TValue)> list)
        => list.GetKeyedLookup<(TKey, TValue), TKey>(PairByKeyEqualityComparer<TKey, TValue>.Default);

    public static KeyedLookup<KeyValuePair<TKey, TValue>, TKey> GetKeyedLookup<TKey, TValue>(this List<KeyValuePair<TKey, TValue>> list)
        => list.GetKeyedLookup((IKeyedEqualityComparer<KeyValuePair<TKey, TValue>, TKey>)PairByKeyEqualityComparer<TKey, TValue>.Default);

    /// <summary>
    /// Returns a view through which modifying the list will keep elements in order.
    /// <br/>
    /// Make sure your list is in order
    /// </summary>
    public static SortedModifier<T> GetSortedModifier<T>(this List<T> list, IComparer<T>? comparer = null)
        => new(list, comparer ?? Comparer<T>.Default);

    /// <summary>
    /// Returns a view through which modifying the list will keep elements in order.
    /// <br/>
    /// Make sure your list is in order
    /// </summary>
    public static RangeSortedModifier<T> GetSortedModifier<T>(this List<T> list, int start, int count, IComparer<T>? comparer = null)
        => new(list, comparer ?? Comparer<T>.Default, start, count);
}
