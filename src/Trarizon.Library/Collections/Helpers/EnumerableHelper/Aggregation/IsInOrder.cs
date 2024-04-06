using System.Numerics;
using Trarizon.Library.Collections.Helpers.Utilities;

namespace Trarizon.Library.Collections.Helpers;
partial class EnumerableHelper
{
    /// <summary>
    /// Determine if the sequence is in order
    /// </summary>
    public static unsafe bool IsInOrder<T>(this IEnumerable<T> source, bool descending) where T : IComparisonOperators<T, T, bool>
    {
        T prev;
        using var enumerator = source.GetEnumerator();
        if (enumerator.MoveNext())
            prev = enumerator.Current;
        else
            // An empty array is in order
            return true;

        delegate*<T, T, bool> compare = descending
         ? &Utility.IsInDescOrder
         : &Utility.IsInAscOrder;

        while (enumerator.MoveNext()) {
            var curr = enumerator.Current;

            if (!compare(prev, curr))
                return false;

            prev = curr;
        }
        return true;
    }

    /// <summary>
    /// Determine if the sequence is in order
    /// </summary>
    public static unsafe bool IsInOrderBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector, bool descending = false) where TKey : IComparisonOperators<TKey, TKey, bool>
    {
        TKey prev;
        using var enumerator = source.GetEnumerator();
        if (enumerator.MoveNext())
            prev = keySelector(enumerator.Current);
        else
            return true;

        delegate*<TKey, TKey, bool> compare = descending
           ? &Utility.IsInDescOrder
           : &Utility.IsInAscOrder;

        while (enumerator.MoveNext()) {
            var curr = keySelector(enumerator.Current);

            if (!compare(prev, curr))
                return false;

            prev = curr;
        }
        return true;
    }

    /// <summary>
    /// Determine if the sequence is in order
    /// </summary>
    public static unsafe bool IsInOrder<T>(this IEnumerable<T> source, IComparer<T>? comparer, bool descending = false)
    {
        T prev;
        using var enumerator = source.GetEnumerator();
        if (enumerator.MoveNext())
            prev = enumerator.Current;
        else
            return true;

        comparer ??= Comparer<T>.Default;

        delegate*<IComparer<T>, T, T, bool> compare = descending
            ? &Utility.IsInDescOrder
            : &Utility.IsInAscOrder;

        while (enumerator.MoveNext()) {
            var curr = enumerator.Current;

            if (!compare(comparer, prev, curr))
                return false;

            prev = curr;
        }
        return true;
    }

    /// <summary>
    /// Determine if the sequence is in order
    /// </summary>
    public static unsafe bool IsInOrderBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector, IComparer<TKey>? comparer, bool descending = false)
    {
        TKey prev;
        using var enumerator = source.GetEnumerator();
        if (enumerator.MoveNext())
            prev = keySelector(enumerator.Current);
        else
            return true;

        comparer ??= Comparer<TKey>.Default;

        delegate*<IComparer<TKey>, TKey, TKey, bool> compare = descending
          ? &Utility.IsInDescOrder
          : &Utility.IsInAscOrder;

        while (enumerator.MoveNext()) {
            var curr = keySelector(enumerator.Current);

            if (!compare(comparer, prev, curr))
                return false;

            prev = curr;
        }
        return true;
    }
}
