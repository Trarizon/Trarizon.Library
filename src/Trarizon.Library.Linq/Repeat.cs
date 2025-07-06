using System.Buffers;
using System.Runtime.CompilerServices;
using Trarizon.Library.Linq.Helpers;
#if NETSTANDARD2_0
using RuntimeHelpers = Trarizon.Library.Linq.Helpers.PfRuntimeHelpers;
#endif

namespace Trarizon.Library.Linq;
public static partial class TraEnumerable
{
    public static IEnumerable<T> Repeat<T>(this IEnumerable<T> source, int count)
    {
        if (count == 1)
            return source;

        if (count <= 0 || source.IsEmptyArray())
            return [];

        if (source is IList<T> ilist) {
            if (source is ListRepeatIterator<T> iterator)
                return iterator.Repeat(count);
            else
                return new ListRepeatIterator<T>(ilist, count);
        }

        return Iterate(source, count);

        static IEnumerable<T> Iterate(IEnumerable<T> source, int count)
        {
            if (source.TryGetNonEnumeratedCount(out var srcCount)) {
                var cache = ArrayPool<T>.Shared.Rent(srcCount);
                try {
                    int ic = 0;
                    foreach (var item in source) {
                        cache[ic++] = item;
                        yield return item;
                    }

                    for (int i = 1; i < count; i++) {
                        foreach (var item in cache) {
                            yield return item;
                        }
                    }
                }
                finally {
                    ArrayPool<T>.Shared.Return(cache, RuntimeHelpers.IsReferenceOrContainsReferences<T>());
                }
            }
            else {
                var cache = new List<T>();
                foreach (var item in source) {
                    cache.Add(item);
                    yield return item;
                }

                for (int i = 1; i < count; i++) {
                    foreach (var item in cache) {
                        yield return item;
                    }
                }
            }
        }
    }

    public static IEnumerable<T> RepeatInterleave<T>(this IEnumerable<T> source, int count)
    {
        if (count == 1)
            return source;
        if (count <= 0 || source.IsEmptyArray())
            return [];
        if (source is IList<T> ilist) {
            if (source is ListRepeatInterleaveIterator<T> iterator)
                return iterator.RepeatInterleave(count);
            return new ListRepeatInterleaveIterator<T>(ilist, count);
        }

        return Iterate(source, count);

        static IEnumerable<T> Iterate(IEnumerable<T> source, int count)
        {
            foreach (var item in source) {
                for (int i = 0; i < count; i++) {
                    yield return item;
                }
            }
        }
    }

    private sealed class ListRepeatIterator<T>(IList<T> list, int repeat) : ListIteratorBase<T>
    {
        private int _repeatIndex;
        private T? _current;

        public override T this[int index]
        {
            get {
                var count = list.Count;
                Throws.IfNegativeOrGreaterThanOrEqual(index, count * repeat);
                return list[index % count];
            }
        }

        public override int Count => list.Count * repeat;

        public override T Current => _current!;

        public override bool MoveNext()
        {
            const int End = MinPreservedState - 1;

            switch (_state) {
                case InitState:
                    _repeatIndex = 0;
                    _state = 0;
                    goto default;
                case End:
                    return false;
                default:
                    if (_state < list.Count) {
                        _current = list[_state];
                        _state++;
                        return true;
                    }
                    var newRepeatIdx = _repeatIndex + 1;
                    if (newRepeatIdx < repeat) {
                        _current = list[0];
                        _state = 1;
                        _repeatIndex = newRepeatIdx;
                        return true;
                    }
                    else {
                        _current = default;
                        _state = End;
                        return false;
                    }
            }
        }

        protected override IteratorBase<T> Clone() => new ListRepeatIterator<T>(list, repeat);

        public ListRepeatIterator<T> Repeat(int count) => new ListRepeatIterator<T>(list, repeat * count);
    }

    private sealed class ListRepeatInterleaveIterator<T>(IList<T> list, int repeat) : ListIteratorBase<T>
    {
        private int _repeatIndex;
        private T? _current;

        public override T this[int index]
        {
            get {
                var count = list.Count;
                Throws.IfNegativeOrGreaterThanOrEqual(index, count * repeat);
                return list[index / count];
            }
        }

        public override int Count => list.Count * repeat;

        public override T Current => _current!;

        public override bool MoveNext()
        {
            const int End = MinPreservedState - 1;

            switch (_state) {
                case InitState:
                    _repeatIndex = 0;
                    _state = 0;
                    _current = list[0];
                    goto default;
                case End:
                    return false;
                default:
                    if (_repeatIndex < repeat) {
                        _repeatIndex++;
                        return true;
                    }
                    var idx = _state + 1;
                    if (idx < list.Count) {
                        _current = list[idx];
                        _repeatIndex = 1;
                        _state = idx;
                        return true;
                    }
                    else {
                        _current = default;
                        _state = End;
                        return false;
                    }
            }
        }

        protected override IteratorBase<T> Clone() => new ListRepeatInterleaveIterator<T>(list, repeat);

        public ListRepeatInterleaveIterator<T> RepeatInterleave(int count) => new ListRepeatInterleaveIterator<T>(list, repeat * count);
    }
}
