using System.Numerics;
using System.Runtime.CompilerServices;

namespace Trarizon.Library.Linq;

public static partial class TraEnumerable
{
#if NET7_0_OR_GREATER

    /// <summary>
    /// Check if sequence is in order using comparison operators
    /// </summary>
    [OverloadResolutionPriority(1)]
    public static bool IsInOrder<T>(this IEnumerable<T> source, bool descending = false) where T : IComparisonOperators<T, T, bool>
        => descending ? IsInOrderByImpl<T, SelfTf<T>, OpDescOrder<T>>(source, new(), new OpDescOrder<T>())
        : IsInOrderByImpl<T, SelfTf<T>, OpAscOrder<T>>(source, new(), new OpAscOrder<T>());

#endif

    /// <summary>
    /// Check if sequence is in ascending order according to comparer
    /// </summary>
    public static bool IsInOrder<T>(this IEnumerable<T> source, IComparer<T>? comparer = null)
        => IsInOrderByImpl<T, SelfTf<T>, CmpOrder<T>>(source, new(), new(comparer ?? Comparer<T>.Default));

#if NET7_0_OR_GREATER

    /// <inheritdoc cref="IsInOrder{T}(IEnumerable{T}, bool)"/>
    public static bool IsInOrderBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector, bool descending = false) where TKey : IComparisonOperators<TKey, TKey, bool>
        => descending ? IsInOrderByImpl<T, TKey, FuncTf<T, TKey>, OpDescOrder<TKey>>(source, new(keySelector), new())
        : IsInOrderByImpl<T, TKey, FuncTf<T, TKey>, OpAscOrder<TKey>>(source, new(keySelector), new());

#endif

    /// <inheritdoc cref="IsInOrder{T}(IEnumerable{T}, IComparer{T}?)"/>
    public static bool IsInOrderBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector, IComparer<TKey>? comparer = null)
        => IsInOrderByImpl<T, TKey, FuncTf<T, TKey>, CmpOrder<TKey>>(source, new(keySelector), new(comparer ?? Comparer<TKey>.Default));

    private static bool IsInOrderByImpl<T, TTransformer, TSorter>(IEnumerable<T> source, TTransformer transformer, TSorter sorter)
        where TTransformer : ITransformer<T, T>
        where TSorter : IOrderComparer<T>
        => IsInOrderByImpl<T, T, TTransformer, TSorter>(source, transformer, sorter);

    private static bool IsInOrderByImpl<T, TKey, TTransformer, TSorter>(IEnumerable<T> source, TTransformer transformer, TSorter sorter)
        where TTransformer : ITransformer<T, TKey>
        where TSorter : IOrderComparer<TKey>
    {
        if (source is T[] { Length: <= 1 })
            return true;

        using var enumerator = source.GetEnumerator();
        if (!enumerator.MoveNext())
            // An empty array is in order
            return true;

        TKey prevKey = transformer.Transform(enumerator.Current);

        while (enumerator.MoveNext())
        {
            var curr = enumerator.Current;
            TKey key = transformer.Transform(curr);
            if (sorter.Greater(prevKey, key))
                return false;
            prevKey = key;
        }
        return true;
    }
}
