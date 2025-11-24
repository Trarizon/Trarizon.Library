using System.Diagnostics;

namespace Trarizon.Library.Linq;

public static partial class TraEnumerable
{
    public static IEnumerable<T> Interleave<T>(this IEnumerable<T> source, IEnumerable<T> other, bool truncateRest = false)
    {
        return (source, other) switch
        {
            (T[] { Length: 0 }, _) => truncateRest ? [] : other,
            (T[] { Length: 1 } arr, _) => other.Prepend(arr[0]),
            (_, T[] { Length: 0 }) => truncateRest ? [] : source,
            (IList<T> llist, IList<T> rlist) => truncateRest
                ? new ListInterleaveTruncateIterator<T>(llist, rlist)
                : new ListInterleaveNoTruncateIterator<T>(llist, rlist),
            _ => Iterate(source, other, truncateRest),
        };

        static IEnumerable<T> Iterate(IEnumerable<T> source, IEnumerable<T> other, bool truncateToShorter)
        {
            using var enumerator = source.GetEnumerator();
            using var enumerator2 = other.GetEnumerator();

            while (enumerator.MoveNext()) {
                if (enumerator2.MoveNext()) {
                    yield return enumerator.Current;
                    yield return enumerator2.Current;
                }
                else {
                    if (!truncateToShorter) {
                        do {
                            yield return enumerator.Current;
                        } while (enumerator.MoveNext());
                    }
                    yield break;
                }
            }
            if (!truncateToShorter) {
                while (enumerator2.MoveNext()) {
                    yield return enumerator2.Current;
                }
            }
        }
    }

    private sealed class ListInterleaveTruncateIterator<T>(IList<T> list, IList<T> other) : ListIteratorBase<T>
    {
        private T? _current;

        public override T this[int index]
        {
            get {
                if (index >= Count) {
                    Throws.ThrowArgumentOutOfRange(nameof(index), index, "Index out of range");
                }
                if (index % 2 == 0)
                    return list[index / 2];
                else
                    return other[index / 2];
            }
        }

        public override int Count => Math.Min(list.Count, other.Count) * 2;

        public override T Current => _current!;

        public override bool MoveNext()
        {
            const int End = MinPreservedState - 1;

            switch (_state) {
                case InitState:
                    _state = 0;
                    goto default;
                case End:
                    return false;
                default:
                    var index = _state / 2;
                    if (_state % 2 == 0) {
                        if (index < list.Count && index < other.Count) {
                            _current = list[index];
                            _state++;
                            return true;
                        }
                        _state = End;
                        _current = default;
                        return false;
                    }
                    else {
                        Debug.Assert(index < list.Count && index < other.Count);
                        _current = other[index];
                        _state++;
                        return true;
                    }
            }
        }

        protected override IteratorBase<T> Clone() => new ListInterleaveTruncateIterator<T>(list, other);

        internal override T TryGetFirst(out bool exists)
        {
            if (list.Count > 0 && other.Count > 0) {
                exists = true;
                return list[0];
            }
            exists = false;
            return default!;
        }

        internal override T TryGetLast(out bool exists)
        {
            var listCount = list.Count;
            var otherCount = other.Count;

            if (listCount > 0 && otherCount > 0) {
                exists = true;
                return other[Math.Min(listCount, otherCount) - 1];
            }
            exists = false;
            return default!;
        }
    }

    private sealed class ListInterleaveNoTruncateIterator<T>(IList<T> list, IList<T> other) : ListIteratorBase<T>
    {
        private T? _current;
        private bool _rest;

        public override T this[int index]
        {
            get {
                var shortCount = Math.Min(list.Count, other.Count);
                if (index < shortCount) {
                    if (index % 2 == 0)
                        return list[index / 2];
                    else
                        return other[index / 2];
                }
                else {
                    if (list.Count > other.Count)
                        return list[index - shortCount];
                    else
                        return other[index - shortCount];
                }
            }
        }

        public override int Count => list.Count + other.Count;

        public override T Current => _current!;

        public override bool MoveNext()
        {
            const int End = MinPreservedState - 1;

            switch (_state) {
                case InitState:
                    _state = 0;
                    goto default;
                case End:
                    return false;
                default: {
                    var index = _state >>> 1;
                    if (_state % 2 == 0) {
                        if (index < list.Count) {
                            _current = list[index];
                            unchecked { _state += _rest ? 2 : 1; }
                            return true;
                        }
                        // even state, but list has no more value, means we start iterate rest of other
                        _rest = true;
                        if (index < other.Count) {
                            _current = other[index];
                            unchecked { _state += 3; }
                            return true;
                        }
                        _state = End;
                        return false;
                    }
                    else {
                        if (index < other.Count) {
                            _current = other[index];
                            unchecked { _state += _rest ? 2 : 1; }
                            return true;
                        }
                        _rest = true;
                        index++;
                        if (index < list.Count) {
                            _current = list[index];
                            unchecked { _state += 3; }
                            return true;
                        }
                        _state = End;
                        return false;
                    }
                }
            }
        }

        protected override IteratorBase<T> Clone() => new ListInterleaveNoTruncateIterator<T>(list, other);

        internal override T TryGetFirst(out bool exists)
        {
            if (list.Count > 0) {
                exists = true;
                return list[0];
            }
            if (other.Count > 0) {
                exists = true;
                return other[0];
            }
            exists = false;
            return default!;
        }

        internal override T TryGetLast(out bool exists)
        {
            var listCount = list.Count;
            var otherCount = other.Count;

            if (listCount <= otherCount) {
                if (otherCount > 0) {
                    exists = true;
                    return other[otherCount - 1];
                }
            }
            else {
                if (listCount > 0) {
                    exists = true;
                    return list[listCount - 1];
                }
            }
            exists = false;
            return default!;
        }
    }
}
