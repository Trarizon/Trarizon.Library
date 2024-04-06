using System.Numerics;

namespace Trarizon.Library.Collections.Helpers;
partial class EnumerableHelper
{
    /// <summary>
    /// Get the minimun and maximun value in one enumeration
    /// </summary>
    public static (T Min, T Max) MinMax<T>(this IEnumerable<T> source, IComparer<T>? comparer = default)
    {
        T min, max;

        using var enumerator = source.GetEnumerator();
        if (enumerator.MoveNext())
            min = max = enumerator.Current;
        else {
            ThrowHelper.ThrowInvalidOperation("Collection is empty");
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

    /// <summary>
    /// Get the minimun and maximun value in one enumeration
    /// </summary>
    public static (T Min, T Max) MinMax<T>(this IEnumerable<T> source) where T : IComparisonOperators<T, T, bool>
    {
        T min, max;

        using var enumerator = source.GetEnumerator();
        if (enumerator.MoveNext())
            min = max = enumerator.Current;
        else {
            ThrowHelper.ThrowInvalidOperation("Collection is empty");
            return default;
        }

        while (enumerator.MoveNext()) {
            var curr = enumerator.Current;
            if (curr < min) min = curr;
            if (curr > max) max = curr;
        }
        return (min, max);
    }

    /// <summary>
    /// Get the minimun and maximun value according to a specific key in one enumeration
    /// </summary>
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
            ThrowHelper.ThrowInvalidOperation("Collection is empty");
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

    /// <summary>
    /// Get the minimun and maximun value according to a specific key in one enumeration
    /// </summary>
    public static (T Min, T Max) MinMaxBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector) where TKey : IComparisonOperators<TKey, TKey, bool>
    {
        T min, max;
        TKey minKey, maxKey;

        using var enumerator = source.GetEnumerator();
        if (enumerator.MoveNext()) {
            min = max = enumerator.Current;
            minKey = maxKey = keySelector(min);
        }
        else {
            ThrowHelper.ThrowInvalidOperation("Collection is empty");
            return default;
        }

        while (enumerator.MoveNext()) {
            var curr = enumerator.Current;
            var key = keySelector(curr);
            if (key < minKey)
                (min, minKey) = (curr, key);
            if (key > maxKey)
                (max, maxKey) = (curr, key);
        }
        return (min, max);
    }

    /// <summary>
    /// Invoke a transform on each element and et the minimun and maximun value in one enumeration
    /// </summary>
    public static (TResult Min, TResult Max) MinMax<T, TResult>(this IEnumerable<T> source, Func<T, TResult> selector, IComparer<TResult>? comparer = default)
    {
        TResult min, max;

        using var enumerator = source.GetEnumerator();
        if (enumerator.MoveNext())
            min = max = selector(enumerator.Current);
        else {
            ThrowHelper.ThrowInvalidOperation("Collection is empty");
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

    /// <summary>
    /// Invoke a transform on each element and et the minimun and maximun value in one enumeration
    /// </summary>
    public static (TResult Min, TResult Max) MinMax<T, TResult>(this IEnumerable<T> source, Func<T, TResult> selector) where TResult : IComparisonOperators<TResult, TResult, bool>
    {
        TResult min, max;

        using var enumerator = source.GetEnumerator();
        if (enumerator.MoveNext())
            min = max = selector(enumerator.Current);
        else {
            ThrowHelper.ThrowInvalidOperation("Collection is empty");
            return default;
        }

        while (enumerator.MoveNext()) {
            var curr = selector(enumerator.Current);
            if (curr < min) min = curr;
            if (curr > max) max = curr;
        }

        return (min, max);
    }
}
