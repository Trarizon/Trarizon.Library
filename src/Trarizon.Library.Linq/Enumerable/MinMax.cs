using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using Trarizon.Library.Linq.Internal;

namespace Trarizon.Library.Linq;

public static partial class TraEnumerable
{
#if NET7_0_OR_GREATER

    [OverloadResolutionPriority(1)]
    [return: NotNullIfNotNull(nameof(defaultValue))]
    public static T? MinOrDefault<T>(this IEnumerable<T> source, T? defaultValue = default) where T : IComparisonOperators<T, T, bool>
    {
        var val = TryMin(source, new ComparisonOperatorsAscCalculator<T>(), out var success);
        return success ? val : defaultValue;
    }

    [OverloadResolutionPriority(1)]
    [return: NotNullIfNotNull(nameof(defaultValue))]
    public static T? MaxOrDefault<T>(this IEnumerable<T> source, T? defaultValue = default) where T : IComparisonOperators<T, T, bool>
    {
        var val = TryMin(source, new ComparisonOperatorsDescCalculator<T>(), out var success);
        return success ? val : defaultValue;
    }

#endif

    [return: NotNullIfNotNull(nameof(defaultValue))]
    public static T? MinOrDefault<T>(this IEnumerable<T> source, IComparer<T>? comparer = null, T? defaultValue = default)
    {
        var val = TryMin(source, new ComparerSorter<T>(comparer ?? Comparer<T>.Default), out var success);
        return success ? val : defaultValue;
    }

    [return: NotNullIfNotNull(nameof(defaultValue))]
    public static T? MaxOrDefault<T>(this IEnumerable<T> source, IComparer<T>? comparer = null, T? defaultValue = default)
    {
        var val = TryMin(source, new ReversedComparerSorter<T>(comparer ?? Comparer<T>.Default), out var success);
        return success ? val : defaultValue;
    }

#if NET7_0_OR_GREATER

    [OverloadResolutionPriority(1)]
    public static (T Min, T Max) MinMax<T>(this IEnumerable<T> source) where T : IComparisonOperators<T, T, bool>
    {
        var ret = TryMinMax(source, new ComparisonOperatorsAscCalculator<T>(), out var success);
        if (!success)
            Throws.CollectionIsEmpty();
        return ret;
    }

    [OverloadResolutionPriority(1)]
    public static (T Min, T Max) MinMaxOrDefault<T>(this IEnumerable<T> source, (T Min, T Max) defaultValue = default) where T : IComparisonOperators<T, T, bool>
    {
        var val = TryMinMax(source, new ComparisonOperatorsAscCalculator<T>(), out var success);
        return success ? val : defaultValue;
    }

    [OverloadResolutionPriority(1)]
    public static (TResult Min, TResult Max) MinMax<T, TResult>(this IEnumerable<T> source, Func<T, TResult> selector) where TResult : IComparisonOperators<TResult, TResult, bool>
    {
        var ret = TryMinMax(source, selector, new ComparisonOperatorsAscCalculator<TResult>(), out var success);
        if (!success)
            Throws.CollectionIsEmpty();
        return ret;
    }

    [OverloadResolutionPriority(1)]
    public static (TResult Min, TResult Max) MinMaxOrDefault<T, TResult>(this IEnumerable<T> source, Func<T, TResult> selector, (TResult Min, TResult Max) defaultValue = default) where TResult : IComparisonOperators<TResult, TResult, bool>
    {
        var val = TryMinMax(source, selector, new ComparisonOperatorsAscCalculator<TResult>(), out var success);
        return success ? val : defaultValue;
    }

    [OverloadResolutionPriority(1)]
    public static (T Min, T Max) MinMaxBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector) where TKey : IComparisonOperators<TKey, TKey, bool>
    {
        var ret = TryMinMaxBy(source, keySelector, new ComparisonOperatorsAscCalculator<TKey>(), out var success);
        if (!success)
            Throws.CollectionIsEmpty();
        return ret;
    }

    [OverloadResolutionPriority(1)]
    public static (T Min, T Max) MinMaxByOrDefault<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector, (T Min, T Max) defaultValue = default) where TKey : IComparisonOperators<TKey, TKey, bool>
    {
        var val = TryMinMaxBy(source, keySelector, new ComparisonOperatorsAscCalculator<TKey>(), out var success);
        return success ? val : defaultValue;
    }

#endif

    /// <summary>
    /// Get the min value and max value in sequence
    /// </summary>
    public static (T Min, T Max) MinMax<T>(this IEnumerable<T> source, IComparer<T>? comparer = null)
    {
        var ret = TryMinMax(source, new ComparerSorter<T>(comparer ?? Comparer<T>.Default), out var success);
        if (!success)
            Throws.CollectionIsEmpty();
        return ret;
    }

    public static (T Min, T Max) MinMaxOrDefault<T>(this IEnumerable<T> source, IComparer<T>? comparer = null, (T Min, T Max) defaultValue = default)
    {
        var val = TryMinMax(source, new ComparerSorter<T>(comparer ?? Comparer<T>.Default), out var success);
        return success ? val : defaultValue;
    }

    public static (TResult Min, TResult Max) MinMax<T, TResult>(this IEnumerable<T> source, Func<T, TResult> selector, IComparer<TResult>? comparer = null)
    {
        var ret = TryMinMax(source, selector, new ComparerSorter<TResult>(comparer ?? Comparer<TResult>.Default), out var success);
        if (!success)
            Throws.CollectionIsEmpty();
        return ret;
    }

    public static (TResult Min, TResult Max) MinMaxOrDefault<T, TResult>(this IEnumerable<T> source, Func<T, TResult> selector, IComparer<TResult>? comparer = null, (TResult Min, TResult Max) defaultValue = default)
    {
        var val = TryMinMax(source, selector, new ComparerSorter<TResult>(comparer ?? Comparer<TResult>.Default), out var success);
        return success ? val : defaultValue;
    }

    /// <inheritdoc cref="MinMax{T}(IEnumerable{T}, IComparer{T})"/>
    public static (T Min, T Max) MinMaxBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector, IComparer<TKey>? comparer = null)
    {
        var ret = TryMinMaxBy(source, keySelector, new ComparerSorter<TKey>(comparer ?? Comparer<TKey>.Default), out var success);
        if (!success)
            Throws.CollectionIsEmpty();
        return ret;
    }

    public static (T Min, T Max) MinMaxByOrDefault<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector, IComparer<TKey>? comparer = null, (T Min, T Max) defaultValue = default)
    {
        var val = TryMinMaxBy(source, keySelector, new ComparerSorter<TKey>(comparer ?? Comparer<TKey>.Default), out var success);
        return success ? val : defaultValue;
    }


    private static (T Min, T Max) TryMinMax<T, TSorter>(IEnumerable<T> source, TSorter sorter, out bool success) where TSorter : IOrderComparer<T>
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
                if (sorter.Less(curr, min))
                    min = curr;
                if (sorter.Greater(curr, max))
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
            if (sorter.Less(curr, min))
                min = curr;
            if (sorter.Greater(curr, max))
                max = curr;
        }
        success = true;
        return (min, max);
    }

    private static (TResult Min, TResult Max) TryMinMax<T, TResult, TSorter>(IEnumerable<T> source, Func<T, TResult> selector, TSorter sorter, out bool success)
        where TSorter : IOrderComparer<TResult>
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
            if (sorter.Less(curr, min))
                min = curr;
            if (sorter.Greater(curr, max))
                max = curr;
        }
        success = true;
        return (min, max);
    }

    private static (T min, T max) TryMinMaxBy<T, TKey, TSorter>(IEnumerable<T> source, Func<T, TKey> keySelector, TSorter sorter, out bool success)
        where TSorter : IOrderComparer<TKey>
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
            if (sorter.Less(key, minKey))
                (min, minKey) = (curr, key);
            if (sorter.Greater(key, maxKey))
                (max, maxKey) = (curr, key);
        }

        success = true;
        return (min, max);
    }

    private static T TryMin<T, TSorter>(IEnumerable<T> source, TSorter sorter, out bool success)
        where TSorter : IOrderComparer<T>
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
                if (sorter.Less(curr, val))
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
            if (sorter.Less(curr, val))
                val = curr;
        }
        success = true;
        return val;
    }

    private readonly struct ComparerSorter<T>(IComparer<T> comparer) : IOrderComparer<T>
    {
        public bool Less(T x, T y) => comparer.Compare(x, y) < 0;
        public bool LessOrEqual(T x, T y) => comparer.Compare(x, y) <= 0;
        public bool Greater(T x, T y) => comparer.Compare(x, y) > 0;
        public bool GreaterOrEqual(T x, T y) => comparer.Compare(x, y) >= 0;
    }

    private readonly struct ReversedComparerSorter<T>(IComparer<T> comparer) : IOrderComparer<T>
    {
        public bool Less(T x, T y) => comparer.Compare(x, y) > 0;
        public bool LessOrEqual(T x, T y) => comparer.Compare(x, y) >= 0;
        public bool Greater(T x, T y) => comparer.Compare(x, y) < 0;
        public bool GreaterOrEqual(T x, T y) => comparer.Compare(x, y) <= 0;
    }

#if NET7_0_OR_GREATER

    private readonly struct ComparisonOperatorsAscCalculator<T> : IOrderComparer<T> where T : IComparisonOperators<T, T, bool>
    {
        public bool Less(T x, T y) => x < y;
        public bool LessOrEqual(T x, T y) => x <= y;
        public bool Greater(T x, T y) => x > y;
        public bool GreaterOrEqual(T x, T y) => x >= y;
    }

    private readonly struct ComparisonOperatorsDescCalculator<T> : IOrderComparer<T> where T : IComparisonOperators<T, T, bool>
    {
        public bool Less(T x, T y) => x > y;
        public bool LessOrEqual(T x, T y) => x >= y;
        public bool Greater(T x, T y) => x < y;
        public bool GreaterOrEqual(T x, T y) => x <= y;
    }

#endif

    #region Polyfills

#if NETSTANDARD

    public static T Min<T>(this IEnumerable<T> source, IComparer<T> comparer)
    {
        var val = TryMin(source, new ComparerSorter<T>(comparer), out var success);
        if (!success)
            Throws.CollectionIsEmpty();
        return val;
    }

    public static T Max<T>(this IEnumerable<T> source, IComparer<T> comparer)
    {
        var val = TryMin(source, new ReversedComparerSorter<T>(comparer), out var success);
        if (!success)
            Throws.CollectionIsEmpty();
        return val;
    }

#endif

    #endregion
}
