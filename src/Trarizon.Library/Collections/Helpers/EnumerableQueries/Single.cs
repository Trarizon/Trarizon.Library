using System.Diagnostics.CodeAnalysis;
using Trarizon.Library.CodeAnalysis.MemberAccess;

namespace Trarizon.Library.Collections.Helpers;
partial class EnumerableQuery
{
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
        => source.TrySingle(predicate).TryGetValue(out value);

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

        [FriendAccess(typeof(EnumerableQuery))]
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
}
