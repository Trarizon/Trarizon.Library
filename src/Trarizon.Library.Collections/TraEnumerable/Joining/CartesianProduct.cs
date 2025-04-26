using Trarizon.Library.Collections.AllocOpt;

namespace Trarizon.Library.Collections;
public static partial class TraEnumerable
{
    public static IEnumerable<(T, T2)> CartesianProduct<T, T2>(this IEnumerable<T> first, IEnumerable<T2> second)
    {
        return (first, second) switch
        {
            (T[] { Length: 0 }, _) or (_, T2[] { Length: 0 }) => [],
            (T[] arr1, T2[] arr2) => new ArrayCartesianProductIterator<T, T2>(arr1, arr2),
            _ => Iterate(first, second),
        };

        static IEnumerable<(T, T2)> Iterate(IEnumerable<T> first, IEnumerable<T2> second)
        {
            using var enumerator = first.GetEnumerator();
            if (!enumerator.MoveNext())
                yield break;

            using var enumerator2 = second.GetEnumerator();

            if (!enumerator2.MoveNext())
                yield break;

            using var cache = new AllocOptList<T2>();
            var cur = enumerator.Current;
            var cur2 = enumerator2.Current;
            yield return (cur, cur2);
            cache.Add(cur2);
            while (enumerator2.MoveNext()) {
                cur2 = enumerator2.Current;
                yield return (cur, cur2);
                cache.Add(cur2);
            }

            while (enumerator.MoveNext()) {
                cur = enumerator.Current;
                foreach (var item in cache) {
                    yield return (cur, item);
                }
            }
        }
    }

    private sealed class ArrayCartesianProductIterator<T, T2>(T[] first, T2[] second) : ListIteratorBase<(T, T2)>
    {
        private (T, T2) _current;
        private int _index2;

        public override (T, T2) this[int index]
        {
            get {
                var div = Math.DivRem(index, second.Length, out var rem);
                return (first[div], second[rem]);
            }
        }

        public override int Count => first.Length * second.Length;

        public override (T, T2) Current => _current;

        public override bool MoveNext()
        {
            const int End = MinPreservedState - 1;

            switch (_state) {
                case InitState:
                    _state = 0;
                    _index2 = -1;
                    goto default;
                case End:
                    return false;
                default:
                    var index2 = _index2 + 1;
                    if (index2 < second.Length) {
                        _current.Item2 = second[index2];
                        _index2 = index2;
                        return true;
                    }
                    var index1 = _state + 1;
                    if (index1 < first.Length) {
                        _current = (first[index1], second[0]);
                        _state = index1;
                        _index2 = 0;
                        return true;
                    }
                    _state = End;
                    return false;
            }
        }

        protected override IteratorBase<(T, T2)> Clone() => new ArrayCartesianProductIterator<T, T2>(first, second);
    }
}
