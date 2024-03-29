﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library.GeneratorToolkit.Extensions;
public static class EnumerableExtensions
{
    #region TryGetNonEnumeratedCount

    public static bool GetNonEnumeratedCount<T>(this IEnumerable<T> source, out int count)
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

    public static bool TryFirst<T>(this IEnumerable<T> source, out T value, T defaultValue = default!)
    {
        if (source is IList<T> list) {
            if (list.Count > 0) {
                value = list[0];
                return true;
            }
            else {
                value = defaultValue;
                return false;
            }
        }

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

    public static bool TryFirst<T>(this IEnumerable<T> source, Func<T, bool> predicate, out T value, T defaultValue = default!)
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

    #endregion

    #region TrySingle

    /// <summary>
    /// Try get first element in sequence,
    /// this method returns false when there is not exactly one element in sequence
    /// </summary>
    /// <remarks>
    /// About nullable analysis:<br/>
    /// There's no warning if <typeparamref name="T"/> is nullable reference type.<br/>
    /// It's hard to provide perfect in all conditions,
    /// so I chose a least-used condition (IMO). At least
    /// it provide correct analysis in most conditions where
    /// <typeparamref name="T"/> is not null
    /// </remarks>
    /// <param name="value">
    /// The only element in sequence,
    /// or <paramref name="defaultValue"/> if sequence is empty or contains more than 1 element.
    /// </param>
    /// <returns><see langword="true"/> if there is exactly one elements in sequence</returns>
    public static bool TrySingle<T>(this IEnumerable<T> source, [NotNullWhen(true), NotNullIfNotNull(nameof(defaultValue))] out T? value, T? defaultValue = default!)
        => source.TrySingleInternal(out value, defaultValue, false);

    /// <summary>
    /// Try get first element satisfying specific condition in sequence,
    /// this method returns false when there is not exactly one element satisfying condition in sequence
    /// </summary>
    /// <remarks>
    /// About nullable analysis:<br/>
    /// There's no warning if <typeparamref name="T"/> is nullable reference type.<br/>
    /// It's hard to provide perfect in all conditions,
    /// so I chose a least-used condition (IMO). At least
    /// it provide correct analysis in most conditions where
    /// <typeparamref name="T"/> is not null
    /// </remarks>
    /// <param name="value">
    /// The only element in sequence,
    /// or <paramref name="defaultValue"/> if sequence is empty or contains more than 1 element.
    /// </param>
    /// <returns><see langword="true"/> if there is exactly one elements satisfying condition in sequence</returns>
    public static bool TrySingle<T>(this IEnumerable<T> source, Func<T, bool> predicate, [NotNullWhen(true), NotNullIfNotNull(nameof(defaultValue))] out T? value, T? defaultValue = default!)
        => source.TryPredicateSingleInternal(predicate, out value, defaultValue, false);

    /// <summary>
    /// Try get first element in sequence or default,
    /// this method returns false when there are more than one element in sequence
    /// </summary>
    /// <remarks>
    /// About nullable analysis:<br/>
    /// There's no warning if <typeparamref name="T"/> is nullable reference type.<br/>
    /// It's hard to provide perfect in all conditions,
    /// so I chose a least-used condition (IMO). At least
    /// it provide correct analysis in most conditions where
    /// <typeparamref name="T"/> is not null
    /// </remarks>
    /// <param name="value">
    /// The only element in sequence,
    /// or <paramref name="defaultValue"/> if sequence is empty or contains more than 1 element.
    /// </param>
    /// <returns><see langword="true"/> if there is one or zero elements in sequence</returns>
    public static bool TrySingleOrNone<T>(this IEnumerable<T> source, [NotNullIfNotNull(nameof(defaultValue))] out T? value, T? defaultValue = default!)
        => source.TrySingleInternal(out value, defaultValue, true);

    /// <summary>
    /// Try get first element satisfying specific condition in sequence or default,
    /// this method returns false when there are more than one element satisfying condition in sequence
    /// </summary>
    /// <remarks>
    /// About nullable analysis:<br/>
    /// There's no warning if <typeparamref name="T"/> is nullable reference type.<br/>
    /// It's hard to provide perfect in all conditions,
    /// so I chose a least-used condition (IMO). At least
    /// it provide correct analysis in most conditions where
    /// <typeparamref name="T"/> is not null
    /// </remarks>
    /// <param name="value">
    /// The only element in sequence,
    /// or <paramref name="defaultValue"/> if sequence is empty or contains more than 1 element.
    /// </param>
    /// <returns><see langword="true"/> if there is one or zero elements satisfying condition in sequence</returns>
    public static bool TrySingleOrNone<T>(this IEnumerable<T> source, Func<T, bool> predicate, [NotNullIfNotNull(nameof(defaultValue))] out T? value, T? defaultValue = default!)
        => source.TryPredicateSingleInternal(predicate, out value, defaultValue, true);

    private static bool TrySingleInternal<T>(this IEnumerable<T> source, out T? value, T? defaultValue, bool resultWhenZero)
    {
        if (source.GetNonEnumeratedCount(out var count)) {
            switch (count) {
                case 0:
                    value = defaultValue;
                    return resultWhenZero;
                case 1:
                    if (source is IList<T> list) {
                        value = list[0];
                    }
                    else {
                        using var e = source.GetEnumerator();
                        e.MoveNext();
                        value = e.Current;
                    }
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

    private static bool TryPredicateSingleInternal<T>(this IEnumerable<T> source, Func<T, bool> predicate, out T? value, T? defaultValue, bool resultWhenNotFound)
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

    #endregion

    public static IEnumerable<(T, T2)> CartesianProduct<T, T2>(this IEnumerable<T> first, IEnumerable<T2> second)
    {
        foreach (var item in first) {
            foreach (var item2 in second) {
                yield return (item, item2);
            }
        }
    }
}
