using System.Collections.Generic;
using System.Diagnostics;

namespace Trarizon.Library.Collections;
partial class TraEnumerable
{
    public static IEnumerable<(T, T)> Adjacent<T>(this IEnumerable<T> source)
    {
        if (source is T[] array) {
            if (array.Length <= 1)
                return [];
            else
                return new ArrayAdjacentIterator<T>(array);
        }

        return Iterate(source);

        static IEnumerable<(T, T)> Iterate(IEnumerable<T> source)
        {
            using var enumerator = source.GetEnumerator();
            if (!enumerator.MoveNext())
                yield break;
            T prev = enumerator.Current;

            while (enumerator.TryMoveNext(out var curr)) {
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
                    _state = 0;
                    goto default;
                case End:
                    return false;
                default:
                    Debug.Assert(_state >= 0);
                    var index2 = _state + 1;
                    if (index2 < source.Length) {
                        _current = (_current.Item2, source[_state + 1]);
                        return true;
                    }
                    _state = End;
                    return false;
            }
        }

        protected override IteratorBase<(T, T)> Clone() => new ArrayAdjacentIterator<T>(source);
    }
}
