using System.Diagnostics;

namespace Trarizon.Library.Linq;

partial class TraEnumerable
{
    public static IEnumerable<(T, T)> Adjacent<T>(this IEnumerable<T> source)
    {
        if (source is T[] array) {
            if (array.Length < 2)
                return [];
            else
                return new ArrayAdjacentIterator<T>(array);
        }
        if (source is IList<T> list)
            return new ListAdjacentIterator<T>(list);

        return Iterate(source);

        static IEnumerable<(T, T)> Iterate(IEnumerable<T> source)
        {
            using var enumerator = source.GetEnumerator();
            if (!enumerator.MoveNext())
                yield break;
            T prev = enumerator.Current;

            while (enumerator.MoveNext()) {
                var curr = enumerator.Current;
                yield return (prev, curr);
                prev = curr;
            }
        }
    }

    private sealed class ArrayAdjacentIterator<T>(T[] source) : ListIteratorBase<(T, T)>
    {
        private (T, T) _current;

        public override (T, T) this[int index] => (source[index], source[index + 1]);

        public override (T, T) Current => _current;

        public override int Count => source.Length - 1;

        public override bool MoveNext()
        {
            const int End = MinPreservedState - 1;

            switch (_state) {
                case InitState:
                    Debug.Assert(source.Length >= 2);
                    _current = (source[0], source[1]);
                    _state = 2;
                    return true;
                case End:
                    return false;
                default:
                    Debug.Assert(_state >= 2);
                    var index2 = _state;
                    if (index2 < source.Length) {
                        _current = (_current.Item2, source[_state]);
                        _state++;
                        return true;
                    }
                    _state = End;
                    return false;
            }
        }

        protected override IteratorBase<(T, T)> Clone() => new ArrayAdjacentIterator<T>(source);

        public override bool Contains((T, T) value)
        {
            var idx = Array.IndexOf(source, value.Item1, 0, source.Length - 1);
            if (idx == -1)
                return false;
            return EqualityComparer<T>.Default.Equals(source[idx + 1], value.Item2);
        }
    }

    private sealed class ListAdjacentIterator<T>(IList<T> source) : ListIteratorBase<(T, T)>
    {
        private (T, T) _current;
        private IEnumerator<T>? _enumerator;

        public override (T, T) this[int index]
        {
            get {
                var lat = source[index + 1]; // If out of range, fail early
                return (source[index], lat);
            }
        }

        public override (T, T) Current => _current;

        public override int Count => Math.Max(0, source.Count - 1);

        public override bool MoveNext()
        {
            const int End = MinPreservedState - 1;

            switch (_state) {
                case InitState:
                    _enumerator = source.GetEnumerator();
                    if (_enumerator.MoveNext()) {
                        _current.Item1 = _enumerator.Current;
                        if (_enumerator.MoveNext()) {
                            _current.Item2 = _enumerator.Current;
                            _state = 0;
                            return true;
                        }
                    }
                    _state = End;
                    return false;
                case End:
                    return false;
                default:
                    Debug.Assert(_enumerator is not null);
                    if (_enumerator!.MoveNext()) {
                        _current = (_current.Item2, _enumerator.Current);
                        return true;
                    }
                    _state = End;
                    return false;
            }
        }

        protected override IteratorBase<(T, T)> Clone() => new ListAdjacentIterator<T>(source);

        protected override void DisposeInternal() => _enumerator?.Dispose();
    }
}