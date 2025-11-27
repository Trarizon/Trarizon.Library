using System.Numerics;
using System.Runtime.CompilerServices;
using Trarizon.Library.Linq.Internal;

namespace Trarizon.Library.Linq;

public static partial class TraEnumerable
{
#if NET7_0_OR_GREATER

    /// <summary>
    /// Check if sequence is in order using comparison operators
    /// </summary>
    [OverloadResolutionPriority(1)]
    public static bool IsInOrder<T>(this IEnumerable<T> source, bool descending = false) where T : IComparisonOperators<T, T, bool>
        => descending ? IsInOrderImpl(source, new ComparisonOperatorsDescCalculator<T>())
        : IsInOrderImpl(source, new ComparisonOperatorsAscCalculator<T>());

#endif

    /// <summary>
    /// Check if sequence is in ascending order according to comparer
    /// </summary>
    public static bool IsInOrder<T>(this IEnumerable<T> source, IComparer<T>? comparer = null)
        => IsInOrderImpl(source, new ComparerSorter<T>(comparer ?? Comparer<T>.Default));

#if NET7_0_OR_GREATER

    /// <inheritdoc cref="IsInOrder{T}(IEnumerable{T}, bool)"/>
    public static bool IsInOrderBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector, bool descending = false) where TKey : IComparisonOperators<TKey, TKey, bool>
        => descending ? IsInOrderByImpl(source, keySelector, new ComparisonOperatorsDescCalculator<TKey>())
        : IsInOrderByImpl(source, keySelector, new ComparisonOperatorsAscCalculator<TKey>());

#endif

    /// <inheritdoc cref="IsInOrder{T}(IEnumerable{T}, IComparer{T}?)"/>
    public static bool IsInOrderBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector, IComparer<TKey>? comparer = null)
        => IsInOrderByImpl(source, keySelector, new ComparerSorter<TKey>(comparer ?? Comparer<TKey>.Default));

    private static bool IsInOrderImpl<T, TSorter>(IEnumerable<T> source, TSorter sorter) where TSorter : IOrderComparer<T>
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
            if (sorter.Greater(prev, curr))
                return false;
            prev = curr;
        }
        return true;
    }

    private static bool IsInOrderByImpl<T, TKey, TSorter>(IEnumerable<T> source, Func<T, TKey> keySelector, TSorter sorter) where TSorter : IOrderComparer<TKey>
    {
        if (source is T[] { Length: <= 1 })
            return true;

        using var enumerator = source.GetEnumerator();
        if (!enumerator.MoveNext())
            // An empty array is in order
            return true;

        TKey prevKey = keySelector(enumerator.Current);

        while (enumerator.MoveNext()) {
            var curr = enumerator.Current;
            TKey key = keySelector(curr);
            if (sorter.Greater(prevKey, key))
                return false;
            prevKey = key;
        }
        return true;
    }
}
