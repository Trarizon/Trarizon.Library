using Microsoft.CodeAnalysis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Trarizon.Library.GeneratorToolkit.Extensions;
public static class EnumerableExtensions
{
    #region TryGetNonEnumeratedCount

    public static bool TryGetNonEnumeratedCount<T>(this IEnumerable<T> source, out int count)
    {
        if (source is ICollection<T> collection) {
            count = collection.Count;
            return true;
        }

        count = default;
        return false;
    }

    #endregion

    #region TryFirst

    public static bool TryFirst<T>(this IEnumerable<T> source, [MaybeNullWhen(false)] out T value)
    {
        if (source is IList<T> list) {
            if (list.Count > 0) {
                value = list[0];
                return true;
            }
            else {
                value = default;
                return false;
            }
        }

        using var enumerator = source.GetEnumerator();

        if (enumerator.MoveNext()) {
            value = enumerator.Current;
            return true;
        }
        else {
            value = default!;
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
        value = default!;
        return false;
    }

    #endregion

    #region TrySingle

    public static bool TrySingle<T>(this IEnumerable<T> source, [MaybeNullWhen(false)] out T first)
    {
        if (source.TrySingleOrNone(out var opt) && opt.HasValue) {
            first = opt.Value;
            return true;
        }
        else {
            first = default;
            return false;
        }
    }

    public static bool TrySingleOrNone<T>(this IEnumerable<T> source, out Optional<T> first)
    {
        using var enumerator = source.GetEnumerator();

        if (!enumerator.MoveNext()) {
            first = default;
            return true;
        }

        if (enumerator.MoveNext()) {
            first = default;
            return false;
        }

        first = enumerator.Current;
        return true;
    }

    public static bool TrySingle<T>(this IEnumerable<T> source, Func<T, bool> predicate, [MaybeNullWhen(false)] out T first)
    {
        if (source.TrySingleOrNone(predicate, out var opt) && opt.HasValue) {
            first = opt.Value;
            return true;
        }
        else {
            first = default;
            return false;
        }
    }

    public static bool TrySingleOrNone<T>(this IEnumerable<T> source, Func<T, bool> predicate, out Optional<T> first)
    {
        using var enumerator = source.GetEnumerator();

        first = default;

        foreach (var item in source) {
            if (predicate(item)) {
                // More than 1
                if (first.HasValue) {
                    first = default;
                    return false;
                }
                else {
                    first = item;
                }
            }
        }

        // 0 or 1
        return true;
    }

    #endregion

    public static IEnumerable<(T, T2)> CartesianProduct<T, T2>(this IEnumerable<T> first, IEnumerable<T2> second)
    {
        foreach (var item in first) {
            foreach (var item2 in second) {
                yield return (item, item2);
            }
        }
    }

    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (var item in source) {
            action(item);
        }
    }

    #region OfFilter

    public static IEnumerable<T> OfNotNull<T>(this IEnumerable<T?> source) where T : class
    {
        foreach (var item in source) {
            if (item is not null)
                yield return item;
        }
    }

    public static IEnumerable<T> OfTypeUntil<T, TExcept>(this IEnumerable source) where TExcept : T
    {
        foreach (var item in source) {
            if (item is T t) {
                if (item is TExcept)
                    yield break;
                else
                    yield return t;
            }
        }
    }

    public static IEnumerable<T> OfTypeWhile<T>(this IEnumerable source)
    {
        foreach (var item in source) {
            if (item is T t)
                yield return t;
            else
                yield break;
        }
    }

    #endregion

    public static IEnumerable<T> EnumerateByWhileNotNull<T>(this T? first, Func<T, T?> nextSelector)
    {
        while (first is not null) {
            yield return first;
            first = nextSelector(first);
        }
    }

    public static IEnumerable<(T, T)> Adjacent<T>(this IEnumerable<T> source)
    {
        using var enumerator = source.GetEnumerator();

        if (!enumerator.MoveNext())
            yield break;

        T first = enumerator.Current;
        T second;
        while (enumerator.MoveNext()) {
            second = enumerator.Current;
            yield return (first, second);
            first = second;
        }
    }

    public static bool IsDistinctBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
    {
        HashSet<TKey> set = [];
        foreach (var item in source) {
            if (!set.Add(keySelector(item)))
                return false;
        }
        return true;
    }

    public static IEnumerable<T> DuplicatesBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
        => source
        .GroupBy(keySelector)
        .Where(g => g.Count() > 1)
        .SelectMany(g => g);

    public static List<T>? ToListIfAny<T>(this IEnumerable<T> source)
    {
        using var enumerator = source.GetEnumerator();
        if (!enumerator.MoveNext())
            return null;
        List<T> result = [enumerator.Current];

        while (enumerator.MoveNext()) {
            result.Add(enumerator.Current);
        }
        return result;
    }
}
