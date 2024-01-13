namespace Trarizon.Library.Collections.Extensions;
partial class EnumerableQuery
{
    // TODO?: Warning when T is null, defaultValue is notnull, return false
    // Same in PopFront

    /// <summary>
    /// Try get first element in sequence,
    /// this method returns false when there is not exactly one element in sequence
    /// </summary>
    /// <param name="value">
    /// The only element in sequence,
    /// or <paramref name="defaultValue"/> if sequence is empty or contains more than 1 element.
    /// </param>
    /// <returns><see langword="true"/> if there is exactly one elements in sequence</returns>
    public static bool TrySingle<T>(this IEnumerable<T> source, out T value, T defaultValue = default!)
        => source.TrySingleInternal(out value, defaultValue, false);

    /// <summary>
    /// Try get first element satisfying specific condition in sequence,
    /// this method returns false when there is not exactly one element satisfying condition in sequence
    /// </summary>
    /// <param name="value">
    /// The only element in sequence,
    /// or <paramref name="defaultValue"/> if sequence is empty or contains more than 1 element.
    /// </param>
    /// <returns><see langword="true"/> if there is exactly one elements satisfying condition in sequence</returns>
    public static bool TrySingle<T>(this IEnumerable<T> source, Func<T, bool> predicate, out T value, T defaultValue = default!)
        => source.TryPredicateSingleInternal(predicate, out value, defaultValue, false);

    /// <summary>
    /// Try get first element in sequence or default,
    /// this method returns false when there are more than one element in sequence
    /// </summary>
    /// <param name="value">
    /// The only element in sequence,
    /// or <paramref name="defaultValue"/> if sequence is empty or contains more than 1 element.
    /// </param>
    /// <returns><see langword="true"/> if there is one or zero elements in sequence</returns>
    public static bool TrySingleOrNone<T>(this IEnumerable<T> source, out T value, T defaultValue = default!)
        => source.TrySingleInternal(out value, defaultValue, true);

    /// <summary>
    /// Try get first element satisfying specific condition in sequence or default,
    /// this method returns false when there are more than one element satisfying condition in sequence
    /// </summary>
    /// <param name="value">
    /// The only element in sequence,
    /// or <paramref name="defaultValue"/> if sequence is empty or contains more than 1 element.
    /// </param>
    /// <returns><see langword="true"/> if there is one or zero elements satisfying condition in sequence</returns>
    public static bool TrySingleOrNone<T>(this IEnumerable<T> source, Func<T, bool> predicate, out T value, T defaultValue = default!)
        => source.TryPredicateSingleInternal(predicate, out value, defaultValue, true);

    private static bool TrySingleInternal<T>(this IEnumerable<T> source, out T value, T defaultValue, bool resultWhenZero)
    {
        if (source is IList<T> list) {
            switch (list.Count) {
                case 0:
                    value = defaultValue;
                    return resultWhenZero;
                case 1:
                    value = list[0];
                    return true;
                default:
                    value = defaultValue;
                    return false;
            }
        }
        using var enumerator = source.GetEnumerator();

        // Zero
        if (!enumerator.MoveNext()) {
            value = defaultValue;
            return resultWhenZero;
        }

        // 1
        value = enumerator.Current;
        if (!enumerator.MoveNext())
            return true;

        // More than 1
        value = defaultValue;
        return false;
    }

    private static bool TryPredicateSingleInternal<T>(this IEnumerable<T> source, Func<T, bool> predicate, out T value, T defaultValue, bool resultWhenNotFound)
    {
        using var enumerator = source.GetEnumerator();

        bool find = false;
        T current = default!;
        while (enumerator.MoveNext()) {
            current = enumerator.Current;
            if (predicate(current)) {
                if (find) {
                    // Multiple
                    value = defaultValue;
                    return false;
                }
                find = true;
            }
        }

        // Single
        if (find) {
            value = current;
            return true;
        }
        // Not found
        else {
            value = defaultValue;
            return resultWhenNotFound;
        }
    }
}
