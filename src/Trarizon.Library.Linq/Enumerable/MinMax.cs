using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library.Linq;

public static partial class TraEnumerable
{
    [return: NotNullIfNotNull(nameof(defaultValue))]
    public static T? MinOrDefault<T, TComparer>(this IEnumerable<T> source, TComparer comparer, T? defaultValue = default) where TComparer : IComparer<T>
    {
        var val = TryMinOrMax(source, new MinCalculator<T>(comparer), out var success);
        return success ? val : defaultValue;
    }

    [return: NotNullIfNotNull(nameof(defaultValue))]
    public static T? MaxOrDefault<T, TComparer>(this IEnumerable<T> source, TComparer comparer, T? defaultValue = default) where TComparer : IComparer<T>
    {
        var val = TryMinOrMax(source, new MaxCalculator<T>(comparer), out var success);
        return success ? val : defaultValue;
    }

    /// <summary>
    /// Get the min value and max value in sequence
    /// </summary>
    public static (T Min, T Max) MinMax<T>(this IEnumerable<T> source, IComparer<T>? comparer = null)
    {
        var ret = TryMinMax(source, comparer ?? Comparer<T>.Default, out var success);
        if (!success)
            Throws.CollectionIsEmpty();
        return ret;
    }

    public static (T Min, T Max) MinMaxOrDefault<T>(this IEnumerable<T> source, IComparer<T>? comparer, (T Min, T Max) defaultValue = default)
    {
        var val = TryMinMax(source, comparer ?? Comparer<T>.Default, out var success);
        return success ? val : defaultValue;
    }

    public static (T Min, T Max) MinMaxOrDefault<T>(this IEnumerable<T> source, (T Min, T Max) defaultValue = default)
        => MinMaxOrDefault(source, (IComparer<T>?)null, defaultValue);

    public static (TResult Min, TResult Max) MinMax<T, TResult>(this IEnumerable<T> source, Func<T, TResult> selector, IComparer<TResult>? comparer = null)
    {
        var ret = TryMinMax(source, selector, comparer ?? Comparer<TResult>.Default, out var success);
        if (!success)
            Throws.CollectionIsEmpty();
        return ret;
    }

    public static (TResult Min, TResult Max) MinMaxOrDefault<T, TResult>(this IEnumerable<T> source, Func<T, TResult> selector, IComparer<TResult>? comparer, (TResult Min, TResult Max) defaultValue = default)
    {
        var val = TryMinMax(source, selector, comparer ?? Comparer<TResult>.Default, out var success);
        return success ? val : defaultValue;
    }

    public static (TResult Min, TResult Max) MinMaxOrDefault<T, TResult>(this IEnumerable<T> source, Func<T, TResult> selector, (TResult Min, TResult Max) defaultValue = default)
        => MinMaxOrDefault(source, selector, null, defaultValue);

    /// <inheritdoc cref="MinMax{T}(IEnumerable{T}, IComparer{T})"/>
    public static (T Min, T Max) MinMaxBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector, IComparer<TKey>? comparer = null)
    {
        var ret = TryMinMaxBy(source, keySelector, comparer ?? Comparer<TKey>.Default, out var success);
        if (!success)
            Throws.CollectionIsEmpty();
        return ret;
    }

    public static (T Min, T Max) MinMaxByOrDefault<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector, IComparer<TKey>? comparer, (T Min, T Max) defaultValue = default)
    {
        var val = TryMinMaxBy(source, keySelector, comparer ?? Comparer<TKey>.Default, out var success);
        return success ? val : defaultValue;
    }

    public static (T Min, T Max) MinMaxByOrDefault<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector, (T Min, T Max) defaultValue = default)
        => MinMaxByOrDefault(source, keySelector, null, defaultValue);

    private static (T Min, T Max) TryMinMax<T>(IEnumerable<T> source, IComparer<T> comparer, out bool success)
    {
        T min, max;
        if (source.TryGetSpan(out var span)) {
            if (span.Length == 0) {
                success = false;
                return default!;
            }
            min = max = span[0];
            for (var i = 1; i < span.Length; i++) {
                var curr = span[i];
                if (comparer.Compare(curr, min) < 0)
                    min = curr;
                if (comparer.Compare(curr, max) > 0)
                    max = curr;
            }
            success = true;
            return (min, max);
        }

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

    private static (TResult Min, TResult Max) TryMinMax<T, TResult>(IEnumerable<T> source, Func<T, TResult> selector, IComparer<TResult> comparer, out bool success)
    {
        TResult min, max;
        using var enumerator = source.GetEnumerator();
        if (!enumerator.MoveNext()) {
            success = false;
            return default;
        }

        min = max = selector(enumerator.Current);

        while (enumerator.MoveNext()) {
            var curr = selector(enumerator.Current);
            if (comparer.Compare(curr, min) < 0)
                min = curr;
            if (comparer.Compare(curr, max) > 0)
                max = curr;
        }
        success = true;
        return (min, max);
    }

    private static (T min, T max) TryMinMaxBy<T, TKey>(IEnumerable<T> source, Func<T, TKey> keySelector, IComparer<TKey> comparer, out bool success)
    {
        T min, max;
        TKey minKey, maxKey;

        using var enumerator = source.GetEnumerator();
        if (!enumerator.MoveNext()) {
            success = false;
            return default;
        }

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

        success = true;
        return (min, max);
    }

    private static T TryMinOrMax<T, TCalculator>(IEnumerable<T> source, TCalculator calculator, out bool success)
        where TCalculator : IMinMaxCalculator<T>
    {
        T val;
        if (source.TryGetSpan(out var span)) {
            if (span.Length == 0) {
                success = false;
                return default!;
            }
            val = span[0];
            for (var i = 1; i < span.Length; i++) {
                var curr = span[i];
                if (calculator.Compare(curr, val))
                    val = curr;
            }
            success = true;
            return val;
        }

        using var enumerator = source.GetEnumerator();
        if (!enumerator.MoveNext()) {
            success = false;
            return default!;
        }

        val = enumerator.Current;
        while (enumerator.MoveNext()) {
            var curr = enumerator.Current;
            if (calculator.Compare(curr, val))
                val = curr;
        }
        success = true;
        return val;
    }

    private interface IMinMaxCalculator<T>
    {
        bool Compare(T x, T y);
    }

    private readonly struct MinCalculator<T>(IComparer<T> comparer) : IMinMaxCalculator<T>
    {
        public bool Compare(T x, T y) => comparer.Compare(x, y) < 0;
    }

    private readonly struct MaxCalculator<T>(IComparer<T> comparer) : IMinMaxCalculator<T>
    {
        public bool Compare(T x, T y) => comparer.Compare(x, y) > 0;
    }

    #region Polyfills

#if NETSTANDARD

    public static T Min<T>(this IEnumerable<T> source, IComparer<T> comparer)
    {
        var val = TryMinOrMax(source, new MinCalculator<T>(comparer), out var success);
        if (!success)
            Throws.CollectionIsEmpty();
        return val;
    }

    public static T Max<T>(this IEnumerable<T> source, IComparer<T> comparer)
    {
        var val = TryMinOrMax(source, new MaxCalculator<T>(comparer), out var success);
        if (!success)
            Throws.CollectionIsEmpty();
        return val;
    }

#endif

    #endregion
}
