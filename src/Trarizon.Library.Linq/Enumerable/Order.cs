namespace Trarizon.Library.Linq;

public static partial class TraEnumerable
{
    /// <summary>
    /// Check if sequence is in ascending order according to comparer
    /// </summary>
    public static bool IsInOrder<T, TComparer>(this IEnumerable<T> source, TComparer comparer) where TComparer : IComparer<T>
    {
        if (source is T[] { Length: <= 1 })
            return true;

        using var enumerator = source.GetEnumerator();
        if (!enumerator.MoveNext())
            // An empty collection is in order
            return true;

        T prev = enumerator.Current;

        while (enumerator.MoveNext()) {
            var curr = enumerator.Current;
            if (comparer.Compare(prev, curr) > 0)
                return false;
            prev = curr;
        }
        return true;
    }

    /// <inheritdoc cref="IsInOrderBy{T, TKey, TComparer}(IEnumerable{T}, Func{T, TKey}, TComparer)"/>
    public static bool IsInOrder<T>(this IEnumerable<T> source)
        => IsInOrder(source, Comparer<T>.Default);

    /// <inheritdoc cref="IsInOrderBy{T, TKey, TComparer}(IEnumerable{T}, Func{T, TKey}, TComparer)"/>
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

        while (enumerator.MoveNext()) {
            var curr = enumerator.Current;
            TKey key = keySelector(curr);
            if (comparer.Compare(prevKey, key) > 0)
                return false;
            prev = curr;
            prevKey = key;
        }
        return true;
    }

    /// <inheritdoc cref="IsInOrderBy{T, TKey, TComparer}(IEnumerable{T}, Func{T, TKey}, TComparer)"/>
    public static bool IsInOrderBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
        => IsInOrderBy(source, keySelector, Comparer<TKey>.Default);
}
