using System.Diagnostics;
using Trarizon.Library.Collections.Helpers;

namespace Trarizon.Library.Collections;
public static partial class TraEnumerable
{
    /// <summary>
    /// Inserts the specified separator between each element of the source sequence.
    /// </summary>
    public static IEnumerable<T> Intersperse<T>(this IEnumerable<T> source, T seperator)
    {
        if (source.TryGetNonEnumeratedCount(out var count) && count <= 1) {
            return source;
        }

        if (source is T[] arr)
            return new ArrayIntersperseIterator<T>(arr, seperator);

        return Iterate(source, seperator);

        static IEnumerable<T> Iterate(IEnumerable<T> source, T seperator)
        {
            using var enumerator = source.GetEnumerator();
            if (!enumerator.MoveNext())
                yield break;

            yield return enumerator.Current;

            while (enumerator.MoveNext()) {
                yield return seperator;
                yield return enumerator.Current;
            }
        }
    }

    private sealed class ArrayIntersperseIterator<T>(T[] list, T seperator) : ListIteratorBase<T>
    {
        private T? _current;

        public override T this[int index]
        {
            get {
                Throws.ThrowIfIndexGreaterThanOrEqual(index, Count);
                if (index % 2 == 0)
                    return list[index / 2];
                else
                    return seperator;
            }
        }

        public override int Count => list.Length * 2 - 1;

        public override T Current => _current!;

        public override bool MoveNext()
        {
            const int End = MinPreservedState - 1;
            const int SeperatorStart = End - 1;

            switch (_state) {
                case InitState:
                    _state = 0;
                    goto InList;
                case End:
                    return false;
                case <= SeperatorStart:
                    goto ReturnSeperator;
                default:
                    goto InList;
            }

        InList:
            Debug.Assert(_state >= 0);
            if (_state < list.Length) {
                _current = list[_state];
                _state = SeperatorStart - _state - 1;
                return true;
            }
            else {
                _current = default;
                _state = End;
                return false;
            }

        ReturnSeperator:
            Debug.Assert(_state <= SeperatorStart);
            var sepIdx = SeperatorStart - _state;
            if (sepIdx < list.Length) {
                _current = seperator;
                _state = sepIdx;
                return true;
            }
            else {
                _current = default;
                _state = End;
                return false;
            }
        }

        protected override IteratorBase<T> Clone() => new ArrayIntersperseIterator<T>(list, seperator);

        public override bool Contains(T value)
        {
            if (EqualityComparer<T>.Default.Equals(value, seperator))
                return true;
            return list.Contains(value);
        }

        public override int IndexOf(T item)
        {
            if (EqualityComparer<T>.Default.Equals(item, seperator)) {
                if (list.Length >= 2)
                    return 1;
                else
                    return -1;
            }

            var idx = Array.IndexOf(list, item);
            if (idx < 0)
                return -1;
            return idx * 2;
        }

        internal override T TryGetFirst(out bool exists)
        {
            exists = true;
            return list[0];
        }

        internal override T TryGetLast(out bool exists)
        {
            exists = true;
            return list[^1];
        }
    }
}
