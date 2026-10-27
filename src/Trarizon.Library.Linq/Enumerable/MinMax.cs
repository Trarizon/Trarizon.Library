using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Trarizon.Library.Linq;

public static partial class TraEnumerable
{
#if NET7_0_OR_GREATER

    [OverloadResolutionPriority(1)]
    [return: NotNullIfNotNull(nameof(defaultValue))]
    public static T? MinOrDefault<T>(this IEnumerable<T> source, T? defaultValue = default) where T : IComparisonOperators<T, T, bool>
    {
        var runner = new MinRunner<T, T, SelfTf<T>, OpAscOrder<T>>();
        var success = TryMinMaxImpl(source, ref runner);
        return success ? runner.Value : defaultValue;
    }

    [OverloadResolutionPriority(1)]
    [return: NotNullIfNotNull(nameof(defaultValue))]
    public static TResult? MinOrDefault<T, TResult>(this IEnumerable<T> score, Func<T, TResult> selector, TResult? defaultValue = default) where TResult : IComparisonOperators<TResult, TResult, bool>
    {
        var runner = new MinRunner<T, TResult, FuncTf<T, TResult>, OpAscOrder<TResult>>(new(selector), default);
        var success = TryMinMaxImpl<T, TResult, MinRunner<T, TResult, FuncTf<T, TResult>, OpAscOrder<TResult>>>(score, ref runner);
        return success ? runner.Value : defaultValue;
    }

    [OverloadResolutionPriority(1)]
    [return: NotNullIfNotNull(nameof(defaultValue))]
    public static T? MaxOrDefault<T>(this IEnumerable<T> source, T? defaultValue = default) where T : IComparisonOperators<T, T, bool>
    {
        var runner = new MinRunner<T, T, SelfTf<T>, OpDescOrder<T>>();
        var success = TryMinMaxImpl(source, ref runner);
        return success ? runner.Value : defaultValue;
    }

    [OverloadResolutionPriority(1)]
    [return: NotNullIfNotNull(nameof(defaultValue))]
    public static TResult? MaxOrDefault<T, TResult>(this IEnumerable<T> score, Func<T, TResult> selector, TResult? defaultValue = default) where TResult : IComparisonOperators<TResult, TResult, bool>
    {
        var runner = new MinRunner<T, TResult, FuncTf<T, TResult>, OpDescOrder<TResult>>(new(selector), default);
        var success = TryMinMaxImpl<T, TResult, MinRunner<T, TResult, FuncTf<T, TResult>, OpDescOrder<TResult>>>(score, ref runner);
        return success ? runner.Value : defaultValue;
    }

#endif

    [return: NotNullIfNotNull(nameof(defaultValue))]
    public static T? MinOrDefault<T>(this IEnumerable<T> source, IComparer<T>? comparer = null, T? defaultValue = default)
    {
        var runner = new MinRunner<T, T, SelfTf<T>, CmpOrder<T>>(default, new(comparer ?? Comparer<T>.Default));
        var success = TryMinMaxImpl(source, ref runner);
        return success ? runner.Value : defaultValue;
    }

    [return: NotNullIfNotNull(nameof(defaultValue))]
    public static T? MaxOrDefault<T>(this IEnumerable<T> source, IComparer<T>? comparer = null, T? defaultValue = default)
    {
        var runner = new MinRunner<T, T, SelfTf<T>, RCmpOrder<T>>(default, new(comparer ?? Comparer<T>.Default));
        var success = TryMinMaxImpl(source, ref runner);
        return success ? runner.Value : defaultValue;
    }

    [return: NotNullIfNotNull(nameof(defaultValue))]
    public static T? MinOrDefault<T>(this IEnumerable<T> source, Func<T, T, int> compare, T? defaultValue = default)
    {
        var runner = new MinRunner<T, T, SelfTf<T>, FuncOrder<T>>(default, new(compare));
        var success = TryMinMaxImpl(source, ref runner);
        return success ? runner.Value : defaultValue;
    }

    [return: NotNullIfNotNull(nameof(defaultValue))]
    public static T? MaxOrDefault<T>(this IEnumerable<T> source, Func<T, T, int> compare, T? defaultValue = default)
    {
        var runner = new MinRunner<T, T, SelfTf<T>, FuncOrder<T>>(default, new(compare));
        var success = TryMinMaxImpl(source, ref runner);
        return success ? runner.Value : defaultValue;
    }

#if NET7_0_OR_GREATER

    [OverloadResolutionPriority(1)]
    public static (T Min, T Max) MinMax<T>(this IEnumerable<T> source) where T : IComparisonOperators<T, T, bool>
    {
        var runner = new MinMaxRunner<T, T, SelfTf<T>, OpAscOrder<T>>(default, default);
        var success = TryMinMaxImpl<T, (T, T), MinMaxRunner<T, T, SelfTf<T>, OpAscOrder<T>>>(source, ref runner);
        if (!success) Throws.CollectionIsEmpty();
        return runner.Value;
    }

    [OverloadResolutionPriority(1)]
    public static (T Min, T Max) MinMaxOrDefault<T>(this IEnumerable<T> source, (T Min, T Max) defaultValue = default) where T : IComparisonOperators<T, T, bool>
    {
        var runner = new MinMaxRunner<T, T, SelfTf<T>, OpAscOrder<T>>(default, default);
        var success = TryMinMaxImpl<T, (T, T), MinMaxRunner<T, T, SelfTf<T>, OpAscOrder<T>>>(source, ref runner);
        return success ? runner.Value : defaultValue;
    }

    [OverloadResolutionPriority(1)]
    public static (TResult Min, TResult Max) MinMax<T, TResult>(this IEnumerable<T> source, Func<T, TResult> selector) where TResult : IComparisonOperators<TResult, TResult, bool>
    {
        var runner = new MinMaxRunner<T, TResult, FuncTf<T, TResult>, OpAscOrder<TResult>>(new(selector), default);
        var success = TryMinMaxImpl<T, (TResult, TResult), MinMaxRunner<T, TResult, FuncTf<T, TResult>, OpAscOrder<TResult>>>(source, ref runner);
        if (!success) Throws.CollectionIsEmpty();
        return runner.Value;
    }

    [OverloadResolutionPriority(1)]
    public static (TResult Min, TResult Max) MinMaxOrDefault<T, TResult>(this IEnumerable<T> source, Func<T, TResult> selector, (TResult Min, TResult Max) defaultValue = default) where TResult : IComparisonOperators<TResult, TResult, bool>
    {
        var runner = new MinMaxRunner<T, TResult, FuncTf<T, TResult>, OpAscOrder<TResult>>(new(selector), default);
        var success = TryMinMaxImpl<T, (TResult, TResult), MinMaxRunner<T, TResult, FuncTf<T, TResult>, OpAscOrder<TResult>>>(source, ref runner);
        return success ? runner.Value : defaultValue;
    }

    [OverloadResolutionPriority(1)]
    public static (T Min, T Max) MinMaxBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector) where TKey : IComparisonOperators<TKey, TKey, bool>
    {
        var runner = new MinMaxByRunner<T, TKey, FuncTf<T, TKey>, OpAscOrder<TKey>>(default, default);
        var success = TryMinMaxImpl<T, (T, T), MinMaxByRunner<T, TKey, FuncTf<T, TKey>, OpAscOrder<TKey>>>(source, ref runner);
        if (!success) Throws.CollectionIsEmpty();
        return runner.Value;
    }

    [OverloadResolutionPriority(1)]
    public static (T Min, T Max) MinMaxByOrDefault<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector, (T Min, T Max) defaultValue = default) where TKey : IComparisonOperators<TKey, TKey, bool>
    {
        var runner = new MinMaxByRunner<T, TKey, FuncTf<T, TKey>, OpAscOrder<TKey>>(default, default);
        var success = TryMinMaxImpl<T, (T, T), MinMaxByRunner<T, TKey, FuncTf<T, TKey>, OpAscOrder<TKey>>>(source, ref runner);
        return success ? runner.Value : defaultValue;
    }

#endif

    /// <summary>
    /// Get the min value and max value in sequence
    /// </summary>
    public static (T Min, T Max) MinMax<T>(this IEnumerable<T> source, IComparer<T>? comparer = null)
    {
        var runner = new MinMaxRunner<T, T, SelfTf<T>, CmpOrder<T>>(default, new(comparer ?? Comparer<T>.Default));
        var success = TryMinMaxImpl<T, (T, T), MinMaxRunner<T, T, SelfTf<T>, CmpOrder<T>>>(source, ref runner);
        if (!success) Throws.CollectionIsEmpty();
        return runner.Value;
    }

    public static (T Min, T Max) MinMaxOrDefault<T>(this IEnumerable<T> source, IComparer<T>? comparer = null, (T Min, T Max) defaultValue = default)
    {
        var runner = new MinMaxRunner<T, T, SelfTf<T>, CmpOrder<T>>(default, new(comparer ?? Comparer<T>.Default));
        var success = TryMinMaxImpl<T, (T, T), MinMaxRunner<T, T, SelfTf<T>, CmpOrder<T>>>(source, ref runner);
        return success ? runner.Value : defaultValue;
    }

    public static (TResult Min, TResult Max) MinMax<T, TResult>(this IEnumerable<T> source, Func<T, TResult> selector, IComparer<TResult>? comparer = null)
    {
        var runner = new MinMaxRunner<T, TResult, FuncTf<T, TResult>, CmpOrder<TResult>>(new(selector), new(comparer ?? Comparer<TResult>.Default));
        var success = TryMinMaxImpl<T, (TResult, TResult), MinMaxRunner<T, TResult, FuncTf<T, TResult>, CmpOrder<TResult>>>(source, ref runner);
        return runner.Value;
    }

    public static (TResult Min, TResult Max) MinMaxOrDefault<T, TResult>(this IEnumerable<T> source, Func<T, TResult> selector, IComparer<TResult>? comparer = null, (TResult Min, TResult Max) defaultValue = default)
    {
        var runner = new MinMaxRunner<T, TResult, FuncTf<T, TResult>, CmpOrder<TResult>>(new(selector), new(comparer ?? Comparer<TResult>.Default));
        var success = TryMinMaxImpl<T, (TResult, TResult), MinMaxRunner<T, TResult, FuncTf<T, TResult>, CmpOrder<TResult>>>(source, ref runner);
        return success ? runner.Value : defaultValue;
    }

    /// <inheritdoc cref="MinMax{T}(IEnumerable{T}, IComparer{T})"/>
    public static (T Min, T Max) MinMaxBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector, IComparer<TKey>? comparer = null)
    {
        var runner = new MinMaxByRunner<T, TKey, FuncTf<T, TKey>, CmpOrder<TKey>>(default, default);
        var success = TryMinMaxImpl<T, (T, T), MinMaxByRunner<T, TKey, FuncTf<T, TKey>, CmpOrder<TKey>>>(source, ref runner);
        if (!success) Throws.CollectionIsEmpty();
        return runner.Value;
    }

    public static (T Min, T Max) MinMaxByOrDefault<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector, IComparer<TKey>? comparer = null, (T Min, T Max) defaultValue = default)
    {
        var runner = new MinMaxByRunner<T, TKey, FuncTf<T, TKey>, CmpOrder<TKey>>(default, default);
        var success = TryMinMaxImpl<T, (T, T), MinMaxByRunner<T, TKey, FuncTf<T, TKey>, CmpOrder<TKey>>>(source, ref runner);
        return success ? runner.Value : defaultValue;
    }

    // FuncOrder

    public static (T Min, T Max) MinMax<T>(this IEnumerable<T> source, Func<T, T, int> compare)
    {
        var runner = new MinMaxRunner<T, T, SelfTf<T>, FuncOrder<T>>(default, new(compare));
        var success = TryMinMaxImpl<T, (T, T), MinMaxRunner<T, T, SelfTf<T>, FuncOrder<T>>>(source, ref runner);
        if (!success) Throws.CollectionIsEmpty();
        return runner.Value;
    }

    public static (T Min, T Max) MinMaxOrDefault<T>(this IEnumerable<T> source, Func<T, T, int> compare, (T Min, T Max) defaultValue = default)
    {
        var runner = new MinMaxRunner<T, T, SelfTf<T>, FuncOrder<T>>(default, new(compare));
        var success = TryMinMaxImpl<T, (T, T), MinMaxRunner<T, T, SelfTf<T>, FuncOrder<T>>>(source, ref runner);
        return success ? runner.Value : defaultValue;
    }

    public static (TResult Min, TResult Max) MinMax<T, TResult>(this IEnumerable<T> source, Func<T, TResult> selector, Func<TResult, TResult, int> compare)
    {
        var runner = new MinMaxRunner<T, TResult, FuncTf<T, TResult>, FuncOrder<TResult>>(new(selector), new(compare));
        var success = TryMinMaxImpl<T, (TResult, TResult), MinMaxRunner<T, TResult, FuncTf<T, TResult>, FuncOrder<TResult>>>(source, ref runner);
        if (!success) Throws.CollectionIsEmpty();
        return runner.Value;
    }

    public static (TResult Min, TResult Max) MinMaxOrDefault<T, TResult>(this IEnumerable<T> source, Func<T, TResult> selector, Func<TResult, TResult, int> compare, (TResult Min, TResult Max) defaultValue = default)
    {
        var runner = new MinMaxRunner<T, TResult, FuncTf<T, TResult>, FuncOrder<TResult>>(new(selector), new(compare));
        var success = TryMinMaxImpl<T, (TResult, TResult), MinMaxRunner<T, TResult, FuncTf<T, TResult>, FuncOrder<TResult>>>(source, ref runner);
        return success ? runner.Value : defaultValue;
    }
}
