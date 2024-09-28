namespace Trarizon.Library.Collections;
partial class TraEnumerable
{
    public static (T Min, T Max) MinMax<T>(this IEnumerable<T> source, IComparer<T>? comparer = default)
    {
        T min, max;

        using var enumerator = source.GetEnumerator();
        if (enumerator.MoveNext())
            min = max = enumerator.Current;
        else {
            TraThrow.NoElement();
            return default;
        }

        comparer ??= Comparer<T>.Default;

        while (enumerator.MoveNext()) {
            var curr = enumerator.Current;
            if (comparer.Compare(curr, min) < 0)
                min = curr;
            if (comparer.Compare(curr, max) > 0)
                max = curr;
        }
        return (min, max);
    }

    public static (T Min, T Max) MinMaxBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector, IComparer<TKey>? comparer = default)
    {
        T min, max;
        TKey minKey, maxKey;

        using var enumerator = source.GetEnumerator();
        if (enumerator.MoveNext()) {
            min = max = enumerator.Current;
            minKey = maxKey = keySelector(min);
        }
        else {
            TraThrow.NoElement();
            return default;
        }

        comparer ??= Comparer<TKey>.Default;

        while (enumerator.MoveNext()) {
            var curr = enumerator.Current;
            var key = keySelector(curr);
            if (comparer.Compare(key, minKey) < 0)
                (min, minKey) = (curr, key);
            if (comparer.Compare(key, maxKey) > 0)
                (max, maxKey) = (curr, key);
        }
        return (min, max);
    }

    public static (TResult Min, TResult Max) MinMax<T, TResult>(this IEnumerable<T> source, Func<T, TResult> selector, IComparer<TResult>? comparer = default)
    {
        TResult min, max;

        using var enumerator = source.GetEnumerator();
        if (enumerator.MoveNext())
            min = max = selector(enumerator.Current);
        else {
            TraThrow.NoElement();
            return default;
        }

        comparer ??= Comparer<TResult>.Default;

        while (enumerator.MoveNext()) {
            var curr = selector(enumerator.Current);
            if (comparer.Compare(curr, min) < 0)
                min = curr;
            if (comparer.Compare(curr, max) > 0)
                max = curr;
        }
        return (min, max);
    }
}
