using System.Numerics;
using Trarizon.Library.Collections.Extensions.Helpers;

namespace Trarizon.Library.Collections.Extensions;
partial class EnumerableQuery
{
#if NET7_0_OR_GREATER
    /// <summary>
    /// Determine if the sequence is in order
    /// </summary>
    public static bool IsInOrder<T>(this IEnumerable<T> source, bool descending) where T : IComparisonOperators<T, T, bool>
    {
        T prev;
        using var enumerator = source.GetEnumerator();
        if (enumerator.MoveNext())
            prev = enumerator.Current;
        else
            // An empty array is in order
            return true;

        while (enumerator.MoveNext()) {
            var curr = enumerator.Current;

            if (!QueryUtil.IsInOrder(prev, curr, descending))
                return false;

            prev = curr;
        }
        return true;
    }

    /// <summary>
    /// Determine if the sequence is in order
    /// </summary>
    public static bool IsInOrderBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector, bool descending = false) where TKey : IComparisonOperators<TKey, TKey, bool>
    {
        TKey prev;
        using var enumerator = source.GetEnumerator();
        if (enumerator.MoveNext())
            prev = keySelector(enumerator.Current);
        else
            return true;

        while (enumerator.MoveNext()) {
            var curr = keySelector(enumerator.Current);

            if (!QueryUtil.IsInOrder(prev, curr, descending))
                return false;

            prev = curr;
        }
        return true;
    }
#endif
    /// <summary>
    /// Determine if the sequence is in order
    /// </summary>
    public static bool IsInOrder<T>(this IEnumerable<T> source, IComparer<T>? comparer = default, bool descending = false)
    {
        T prev;
        using var enumerator = source.GetEnumerator();
        if (enumerator.MoveNext())
            prev = enumerator.Current;
        else
            return true;

        comparer ??= Comparer<T>.Default;

        while (enumerator.MoveNext()) {
            var curr = enumerator.Current;

            if (!QueryUtil.IsInOrder(prev, curr, comparer, descending))
                return false;

            prev = curr;
        }
        return true;
    }

    /// <summary>
    /// Determine if the sequence is in order
    /// </summary>
    public static bool IsInOrderBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector, IComparer<TKey>? comparer = default, bool descending = false)
    {
        TKey prev;
        using var enumerator = source.GetEnumerator();
        if (enumerator.MoveNext())
            prev = keySelector(enumerator.Current);
        else
            return true;

        comparer ??= Comparer<TKey>.Default;

        while (enumerator.MoveNext()) {
            var curr = keySelector(enumerator.Current);

            if (!QueryUtil.IsInOrder(prev, curr, comparer, descending))
                return false;

            prev = curr;
        }
        return true;
    }
}
