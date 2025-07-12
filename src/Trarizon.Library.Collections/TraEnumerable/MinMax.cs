using Trarizon.Library.Collections.Helpers;

namespace Trarizon.Library.Collections;
public static partial class TraEnumerable
{
    /// <summary>
    /// Get the min value and max value in sequence
    /// </summary>
    public static (T Min, T Max) MinMax<T, TComparer>(this IEnumerable<T> source, TComparer comparer) where TComparer : IComparer<T>
    {
        var ret = TryMinMax(source, comparer, out var success);
        if (!success)
            Throws.CollectionIsEmpty();
        return ret;
    }

    /// <inheritdoc cref="MinMax{T, TComparer}(IEnumerable{T}, TComparer)"/>
    public static (T Min, T Max) MinMax<T>(this IEnumerable<T> source)
        => MinMax(source, Comparer<T>.Default);

    /// <summary>
    /// Get the min value and max value in sequence
    /// </summary>
    /// <returns>
    /// Min value and max value in collection, <see langword="null"/> if <paramref name="source"/> is empty
    /// </returns>
    public static (T Min, T Max)? MinMaxOrNull<T, TComparer>(this IEnumerable<T> source, TComparer comparer) where TComparer : IComparer<T>
    {
        var ret = TryMinMax(source, comparer, out var success);
        if (!success)
            return null;
        return ret;
    }

    /// <inheritdoc cref="MinMaxOrNull{T, TComparer}(IEnumerable{T}, TComparer)"/>
    public static (T Min, T Max)? MinMaxOrNull<T>(this IEnumerable<T> source)
        => source.MinMaxOrNull(Comparer<T>.Default);

    /// <inheritdoc cref="MinMax{T, TComparer}(IEnumerable{T}, TComparer)"/>
    public static (T Min, T Max) MinMaxBy<T, TKey, TComparer>(this IEnumerable<T> source, Func<T, TKey> keySelector, TComparer comparer) where TComparer : IComparer<TKey>
    {
        T min, max;
        TKey minKey, maxKey;

        using var enumerator = source.GetEnumerator();
        if (!enumerator.MoveNext())
            Throws.CollectionIsEmpty();

        min = max = enumerator.Current;
        minKey = maxKey = keySelector(min);

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

    /// <inheritdoc cref="MinMax{T, TComparer}(IEnumerable{T}, TComparer)"/>
    public static (T Min, T Max) MinMaxBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
        => MinMaxBy(source, keySelector, Comparer<TKey>.Default);

    /// <inheritdoc cref="MinMax{T, TComparer}(IEnumerable{T}, TComparer)"/>
    public static (TResult Min, TResult Max) MinMax<T, TResult>(this IEnumerable<T> source, Func<T, TResult> selector)
        => MinMax(source, selector, Comparer<TResult>.Default);

    /// <inheritdoc cref="MinMax{T, TComparer}(IEnumerable{T}, TComparer)"/>
    public static (TResult Min, TResult Max) MinMax<T, TResult, TComparer>(this IEnumerable<T> source, Func<T, TResult> selector, TComparer comparer) where TComparer : IComparer<TResult>
    {
        TResult min, max;

        using var enumerator = source.GetEnumerator();
        if (!enumerator.MoveNext())
            Throws.CollectionIsEmpty();

        min = max = selector(enumerator.Current);

        while (enumerator.MoveNext()) {
            var curr = selector(enumerator.Current);
            if (comparer.Compare(curr, min) < 0)
                min = curr;
            if (comparer.Compare(curr, max) > 0)
                max = curr;
        }
        return (min, max);
    }

    private static (T Min, T Max) TryMinMax<T, TComparer>(IEnumerable<T> source, TComparer comparer, out bool success) where TComparer : IComparer<T>
    {
        T min, max;

        using var enumerator = source.GetEnumerator();
        if (!enumerator.MoveNext()) {
            success = false;
            return default;
        }

        min = max = enumerator.Current;

        while (enumerator.MoveNext()) {
            var curr = enumerator.Current;
            if (comparer.Compare(curr, min) < 0)
                min = curr;
            if (comparer.Compare(curr, max) > 0)
                max = curr;
        }
        success = true;
        return (min, max);
    }
}
