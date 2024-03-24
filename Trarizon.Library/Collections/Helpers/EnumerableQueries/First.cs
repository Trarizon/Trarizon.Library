using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace Trarizon.Library.Collections.Helpers;
partial class EnumerableQuery
{
    public static bool TryFirst<T>(this IEnumerable<T> source, [MaybeNullWhen(false)] out T value)
        => source.TryFirst(out value, default!);

    public static bool TryFirst<T>(this IEnumerable<T> source, out T value, T defaultValue)
    {
        if (source.TryGetNonEnumeratedCount(out var count)) {
            if (count > 0) {
                if (source is IList<T> list) {
                    value = list[0];
                    return true;
                }
                goto ByEnumerate;
            }
            else {
                value = defaultValue;
                return false;
            }
        }

    ByEnumerate:

        using var enumerator = source.GetEnumerator();

        if (enumerator.MoveNext()) {
            value = enumerator.Current;
            return true;
        }
        else {
            value = defaultValue;
            return false;
        }
    }

    public static bool TryFirst<T>(this IEnumerable<T> source, Func<T, bool> predicate, [MaybeNullWhen(false)] out T value)
        => source.TryFirst(predicate, out value, default!);

    public static bool TryFirst<T>(this IEnumerable<T> source, Func<T, bool> predicate, out T value, T defaultValue)
    {
        using var enumerator = source.GetEnumerator();

        while (enumerator.MoveNext()) {
            var current = enumerator.Current;
            if (predicate(current)) {
                value = current;
                return true;
            }
        }
        value = defaultValue;
        return false;
    }

    /// <summary>
    /// Find the first item has priority &gt;= <paramref name="maxProirity"/>,
    /// if not found, return the first item with greater priority,
    /// if <paramref name="source"/> is empty, return default
    /// </summary>
    public static (TPriority Priority, T? Value) FirstByMaxPriorityOrDefault<T, TPriority>(this IEnumerable<T> source, TPriority maxProirity, Func<T, TPriority> prioritySelector, IComparer<TPriority>? comparer = null)
    {
        using var enumerator = source.GetEnumerator();

        TPriority priority;
        T? value;

        comparer ??= Comparer<TPriority>.Default;

        if (enumerator.MoveNext()) {
            value = enumerator.Current;
            priority = prioritySelector(value);
            if (comparer.Compare(priority, maxProirity) >= 0)
                return (priority, value);
        }
        else {
            return default;
        }

        while (enumerator.MoveNext()) {
            var current = enumerator.Current;
            var curPriority = prioritySelector(current);

            if (comparer.Compare(curPriority, maxProirity) >= 0)
                return (curPriority, current);
            if (comparer.Compare(curPriority, priority) > 0) {
                value = current;
                priority = curPriority;
            }
        }
        return (priority, value);
    }

    /// <summary>
    /// Find the first item has priority &gt;= <paramref name="maxProirity"/>,
    /// if not found, return the first item with greater priority,
    /// if <paramref name="source"/> is empty, return default
    /// </summary>
    public static (TPriority Priority, T? Value) FirstByMaxPriorityOrDefault<T, TPriority>(this IEnumerable<T> source, TPriority maxPriority, Func<T, TPriority> prioritySelector) where TPriority : IComparisonOperators<TPriority, TPriority, bool>
    {
        using var enumerator = source.GetEnumerator();

        TPriority priority;
        T? value;

        if (enumerator.MoveNext()) {
            value = enumerator.Current;
            priority = prioritySelector(value);
            if (priority >= maxPriority)
                return (priority, value);
        }
        else {
            return default;
        }

        while (enumerator.MoveNext()) {
            var current = enumerator.Current;
            var curPriority = prioritySelector(current);

            if (curPriority >= maxPriority)
                return (curPriority, current);
            if (curPriority > priority) {
                value = current;
                priority = curPriority;
            }
        }
        return (priority, value);
    }
}
