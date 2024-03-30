using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library.SourceGenerator.Toolkit.Extensions;
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

    public static bool TryFirst<T>(this IEnumerable<T> source, Func<T, bool> predicate, out T value)
        => source.TryFirst(predicate, out value, default!);

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
    public static bool TrySingle<T>(this IEnumerable<T> source, [MaybeNullWhen(false)] out T value)
        => source.TrySingle().TryGetValue(out value);

    /// <summary>
    /// Try get first element satisfying specific condition in sequence or default,
    /// this method returns false when there are more than one element satisfying condition in sequence
    /// </summary>
    public static bool TrySingle<T>(this IEnumerable<T> source, Func<T, bool> predicate, [MaybeNullWhen(false)] out T value)
        => source.TrySingle(predicate, out value);

    /// <summary>
    /// Try get first element in sequence,
    /// this method returns false when there is not exactly one element in sequence
    /// </summary>
    public static SingleOptional<T> TrySingle<T>(this IEnumerable<T> source)
    {
        if (source.TryGetNonEnumeratedCount(out var count)) {
            switch (count) {
                case 0:
                    return new(default, SingleOptionalKind.Empty);
                case 1:
                    if (source is IList<T> list) {
                        return new(list[0], SingleOptionalKind.Single);
                    }
                    else {
                        using var e = source.GetEnumerator();
                        e.MoveNext();
                        return new(e.Current, SingleOptionalKind.Single);
                    }
                default:
                    return new(default, SingleOptionalKind.Multiple);
            }
        }

        using var enumerator = source.GetEnumerator();

        // Zero
        if (!enumerator.MoveNext()) {
            return new(default, SingleOptionalKind.Empty);
        }

        // 1
        var value = enumerator.Current;
        if (!enumerator.MoveNext())
            return new(value, SingleOptionalKind.Single);

        // More than 1
        return new(default, SingleOptionalKind.Multiple);
    }

    /// <summary>
    /// Try get first element satisfying specific condition in sequence or default,
    /// this method returns false when there are more than one element satisfying condition in sequence
    /// </summary>
    public static SingleOptional<T> TrySingle<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        using var enumerator = source.GetEnumerator();

        bool found = false;
        T current = default!;
        while (enumerator.MoveNext()) {
            current = enumerator.Current;
            if (predicate(current)) {
                if (found) {
                    // Multiple
                    return new(default, SingleOptionalKind.Multiple);
                }
                found = true;
            }
        }

        // Single
        if (found) {
            return new(current, SingleOptionalKind.Single);
        }
        // Not found
        else {
            return new(default, SingleOptionalKind.Empty);
        }
    }


    public readonly struct SingleOptional<T>
    {
        private readonly SingleOptionalKind _kind;
        private readonly T? _value;

        internal SingleOptional(T? value, SingleOptionalKind kind)
        {
            _kind = kind;
            _value = value;
        }

        public bool HasValue => _kind is SingleOptionalKind.Single;

        public SingleOptionalKind ResultKind => _kind;

        public bool TryGetValue([MaybeNullWhen(false)] out T value)
        {
            value = _value;
            return HasValue;
        }

        public bool TryGetValue(out T value, T defaultValue)
        {
            var rtn = _kind is SingleOptionalKind.Single;
            value = rtn ? _value! : defaultValue;
            return rtn;
        }

        public bool TryGetValueOrNone(out T? value, T? defaultValue = default)
        {
            var rtn = _kind is not SingleOptionalKind.Multiple;
            value = rtn ? _value! : defaultValue;
            return rtn;
        }

        public T? GetValueOrDefault() => _value;
    }

    public enum SingleOptionalKind
    {
        Empty,
        Single,
        Multiple,
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

    public static IEnumerable<T> OfTypeUntil<T, TExcept>(this IEnumerable source) where TExcept : T
    {
        foreach (var item in source) {
            if (item is T typed) {
                if (item is TExcept)
                    yield break;
                else
                    yield return typed;
            }
        }
    }

    public static IEnumerable<(T, T)> Adjacent<T>(this IEnumerable<T> source)
    {
        using var enumerator = source.GetEnumerator();
        if (!enumerator.MoveNext())
            yield break;

        var prev = enumerator.Current;

        while (enumerator.MoveNext()) {
            var cur = enumerator.Current;
            yield return (prev, cur);
            prev = cur;
        }
    }

    public static IEnumerable<T> EnumerateByWhileNotNull<T>(this T? first, Func<T, T?> nextSelector)
    {
        while (first is not null) {
            yield return first;
            first = nextSelector(first);
        }
    }

    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (var item in source) {
            action(item);
        }
    }

    public static IEnumerable<T> OfNotNull<T>(this IEnumerable<T?> source) where T : class
    {
        foreach (var item in source) {
            if (item is not null)
                yield return item;
        }
    }
}
