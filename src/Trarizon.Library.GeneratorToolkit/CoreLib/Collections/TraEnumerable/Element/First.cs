using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library.GeneratorToolkit.CoreLib.Collections;
partial class TraEnumerable
{
    /// <remarks>
    /// Official LinQ has some internal optimizations for linq chain on <see cref="Enumerable.FirstOrDefault{TSource}(IEnumerable{TSource})"/>,
    /// so i suggest not using this if <see cref="Enumerable.FirstOrDefault{TSource}(IEnumerable{TSource})"/> meets your requirements.
    /// </remarks>
    public static bool TryFirst<T>(this IEnumerable<T> source, [MaybeNullWhen(false)] out T value)
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
                value = default;
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
            value = default;
            return false;
        }
    }

    public static bool TryFirst<T>(this IEnumerable<T> source, Func<T, bool> predicate, [MaybeNullWhen(false)] out T value)
    {
        using var enumerator = source.GetEnumerator();

        while (enumerator.MoveNext()) {
            var current = enumerator.Current;
            if (predicate(current)) {
                value = current;
                return true;
            }
        }
        value = default;
        return false;
    }

    /// <summary>
    /// Find the first item has priority &gt;= <paramref name="maxProirity"/>,
    /// if not found, return the first item with greatest priority
    /// </summary>
    public static (TPriority Priority, T? Value) FirstByMaxPriorityOrDefault<T, TPriority>(this IEnumerable<T> source, TPriority maxProirity, Func<T, TPriority> prioritySelector, IComparer<TPriority>? comparer = null)
    {
        using var enumerator = source.GetEnumerator();

        TPriority priority;
        T? value;

        comparer ??= Comparer<TPriority>.Default;

        if (!enumerator.MoveNext()) {
            return default;
        }

        value = enumerator.Current;
        priority = prioritySelector(value);
        if (comparer.Compare(priority, maxProirity) >= 0)
            return (priority, value);

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
}
