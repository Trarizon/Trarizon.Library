using System.Diagnostics;
using Trarizon.Library.Collections.Helpers;

namespace Trarizon.Library.Collections;
public static partial class TraEnumerable
{
    public static IEnumerable<T> Interleave<T>(this IEnumerable<T> source, IEnumerable<T> other, bool truncateToShorter = false)
    {
        if (source.TryGetNonEnumeratedCount(out var count)) {
            if (count == 0)
                return truncateToShorter ? [] : other;
            if (count == 1)
                return other.Prepend(source.First());
            if (other.TryGetNonEnumeratedCount(out var count2) && count2 == 0)
                return truncateToShorter ? [] : source;
            if (source is IList<T> listl && other is IList<T> listr)
                return new ListInterleaveIterator<T>(listl, listr, truncateToShorter);
        }
        else if (other.TryGetNonEnumeratedCount(out var count2) && count2 == 0)
            return truncateToShorter ? [] : source;

        return Iterate(source, other, truncateToShorter);

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

    private sealed class ListInterleaveIterator<T>(IList<T> list, IList<T> other, bool truncateToShorter) : ListIteratorBase<T>
    {
        private T? _current;

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
                else if (truncateToShorter) {
                    Throws.ThrowArgumentOutOfRange(nameof(index), index, "Index out of range");
                    return default!;
                }
                else {
                    if (list.Count > other.Count)
                        return list[index - shortCount];
                    else
                        return other[index - shortCount];
                }
            }
        }

        public override int Count => truncateToShorter ? Math.Min(list.Count, other.Count) * 2 : list.Count + other.Count;

        public override T Current => _current!;

        public override bool MoveNext()
        {
            const int End = MinPreservedState - 1;
            const int SrcRest = End - 1;
            const int OtherRest = SrcRest - 1;
            const int OtherStart = OtherRest - 1;

            switch (_state) {
                case InitState:
                    _state = 0;
                    goto default;
                case End:
                    return false;
                case SrcRest:
                case OtherRest:
                    if (_state < other.Count) {
                        _current = other[_state];
                        _state++;
                        return true;
                    }
                    else {
                        _current = default;
                        _state = End;
                        return false;
                    }
                case <= OtherStart:
                    var idx = OtherStart - _state;
                    if (idx < other.Count) {
                        var nxtSrcIdx = idx + 1;
                        _current = other[idx];
                        if (nxtSrcIdx >= list.Count)
                            _state--;
                        else
                            _state = nxtSrcIdx;
                        return true;
                    }
                    else {
                        Debug.Assert(other.Count >= list.Count);
                        _current = default;
                        _state = End;
                        return false;
                    }
                default:
                    Debug.Assert(_state >= 0);
                    if (_state < list.Count) {
                        if (_state < other.Count) {
                            _current = list[_state];
                            _state = OtherStart - _state;
                            return true;
                        }
                        else if (truncateToShorter) {
                            _current = default;
                            _state = End;
                            return false;
                        }
                        else {
                            _current = list[_state];
                            _state++;
                            return true;
                        }
                    }
                    else {
                        Debug.Assert(list.Count > other.Count);
                        _current = default;
                        _state = End;
                        return false;
                    }
            }
        }

        protected override IteratorBase<T> Clone() => new ListInterleaveIterator<T>(list, other, truncateToShorter);
    }
}
