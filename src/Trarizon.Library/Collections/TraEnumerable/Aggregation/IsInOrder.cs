namespace Trarizon.Library.Collections;
partial class TraEnumerable
{
    public static unsafe bool IsInOrder<T>(this IEnumerable<T> source, IComparer<T>? comparer = null, bool descending = false)
    {
        using var enumerator = source.GetEnumerator();
        if (!enumerator.MoveNext())
            // An empty array is in order
            return true;

        T prev = enumerator.Current;
        comparer ??= Comparer<T>.Default;
        delegate*<T, T, IComparer<T>, int> cmp = descending
            ? &TraAlgorithm.Utils.CmpReverse
            : &TraAlgorithm.Utils.Cmp;

        while (enumerator.TryMoveNext(out var curr)) {
            if (cmp(prev, curr, comparer) > 0)
                return false;
            prev = curr;
        }
        return true;
    }

    public static unsafe bool IsInOrderBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector, IComparer<TKey>? comparer = null, bool descending = false)
    {
        using var enumerator = source.GetEnumerator();
        if (!enumerator.MoveNext())
            // An empty array is in order
            return true;

        T prev = enumerator.Current;
        TKey prevKey = keySelector(prev);
        comparer ??= Comparer<TKey>.Default;
        delegate*<TKey, TKey, IComparer<TKey>, int> cmp = descending
            ? &TraAlgorithm.Utils.CmpReverse : &TraAlgorithm.Utils.Cmp;

        while (enumerator.TryMoveNext(out var curr)) {
            TKey key = keySelector(curr);
            if (cmp(prevKey, key, comparer) > 0)
                return false;
            prev = curr;
            prevKey = key;
        }
        return true;
    }
}
