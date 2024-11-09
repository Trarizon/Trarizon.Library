namespace Trarizon.Library.Collections;
partial class TraEnumerable
{
    public static bool IsInOrder<T>(this IEnumerable<T> source)
        => IsInOrder(source, Comparer<T>.Default);

    public static bool IsInOrder<T, TComparer>(this IEnumerable<T> source, TComparer comparer) where TComparer : IComparer<T>
    {
        if (source is T[] { Length: <= 1 }) 
            return true;

        using var enumerator = source.GetEnumerator();
        if (!enumerator.MoveNext())
            // An empty collection is in order
            return true;

        T prev = enumerator.Current;

        while (enumerator.TryMoveNext(out var curr)) {
            if (comparer.Compare(prev, curr) > 0)
                return false;
            prev = curr;
        }
        return true;
    }

    public static bool IsInOrderBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
        => IsInOrderBy(source, keySelector, Comparer<TKey>.Default);

    public static bool IsInOrderBy<T, TKey, TComparer>(this IEnumerable<T> source, Func<T, TKey> keySelector, TComparer comparer) where TComparer : IComparer<TKey>
    {
        if (source is T[] { Length: <= 1 })
            return true;

        using var enumerator = source.GetEnumerator();
        if (!enumerator.MoveNext())
            // An empty array is in order
            return true;

        T prev = enumerator.Current;
        TKey prevKey = keySelector(prev);

        while (enumerator.TryMoveNext(out var curr)) {
            TKey key = keySelector(curr);
            if (comparer.Compare(prevKey, key) > 0)
                return false;
            prev = curr;
            prevKey = key;
        }
        return true;
    }
}
