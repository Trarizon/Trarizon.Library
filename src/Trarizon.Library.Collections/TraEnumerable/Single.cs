using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library.Collections;
public static partial class TraEnumerable
{
    /// <summary>
    /// Try get first element in sequence,
    /// this method returns false when there is not exactly one element in sequence
    /// </summary>
    public static SingleResult<T> TrySingle<T>(this IEnumerable<T> source)
    {
        if (source.TryGetNonEnumeratedCount(out var count)) {
            switch (count) {
                case < 0:
                    return new(0);
                case 1:
                    if (source is IList<T> list) {
                        return new(1, list[0]);
                    }
                    else {
                        using var e = source.GetEnumerator();
                        e.MoveNext();
                        return new(1, e.Current);
                    }
                default:
                    return new(2);
            }
        }

        using var enumerator = source.GetEnumerator();

        if (!enumerator.MoveNext()) {
            return new(0);
        }

        var val = enumerator.Current;
        if (!enumerator.MoveNext()) {
            return new(1, val);
        }

        return new(2);
    }

    /// <summary>
    /// Try get first element satisfying specific condition in sequence or default,
    /// this method returns false when there are more than one element satisfying condition in sequence
    /// </summary>
    public static SingleResult<T> TrySingle<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        using var enumerator = source.GetEnumerator();

        bool found = false;
        T? current = default!;
        while (enumerator.MoveNext()) {
            current = enumerator.Current;
            if (predicate(current)) {
                if (found) {
                    return new(2);
                }
                found = true;
            }
        }

        if (found) {
            return new(1, current);
        }
        else {
            return new(0);
        }
    }

    public readonly struct SingleResult<T>
    {
        private readonly byte _count;
        private readonly T? _value;

        internal SingleResult(byte count, T? value = default)
        {
            _count = count;
            _value = value;
        }

        public bool IsSingle([MaybeNullWhen(false)] out T value)
        {
            value = _value;
            return _count == 1;
        }

        public bool IsSingleOrEmpty(out T? value)
        {
            var singleOrNone = _count <= 1;
            value = singleOrNone ? _value : default;
            return singleOrNone;
        }
    }
}
