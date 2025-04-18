using System.Diagnostics;
using Trarizon.Library.Collections.Generic;

namespace Trarizon.Library.Collections;
public static partial class TraEnumerable
{
    public static IEnumerable<(T Item, int AheadCount)> LookAhead<T>(this IEnumerable<T> source, int maxAheadCount)
    {
        if (source is T[] arr) {
            if (arr.Length == 0)
                return [];
            else
                return new ListLookAheadIterator<T>(arr, maxAheadCount);
        }
        else if (source.TryGetNonEnumeratedCount(out var count)) {
            return new CollectionLookAheadIterator<T>(source, count, maxAheadCount);
        }

        if (maxAheadCount == 0)
            return source.Select(x => (x, 0));
        if (maxAheadCount == 1)
            return Iterate1(source);
        return Iterate(source, maxAheadCount);

        static IEnumerable<(T, int)> Iterate1(IEnumerable<T> source)
        {
            T prev;
            using var enumerator = source.GetEnumerator();
            if (!enumerator.MoveNext())
                yield break;
            prev = enumerator.Current;

            while (enumerator.MoveNext()) {
                yield return (prev, 1);
                prev = enumerator.Current;
            }
            yield return (prev, 0);
        }

        static IEnumerable<(T, int)> Iterate(IEnumerable<T> source, int maxAheadCount)
        {
            var queue = new RingQueue<T>(maxAheadCount);

            using var enumerator = source.GetEnumerator();
            for (int i = 0; i < queue.MaxCount; i++) {
                if (enumerator.MoveNext())
                    queue.Enqueue(enumerator.Current);
                else {
                    goto IterateQueue;
                }
            }

            while (enumerator.MoveNext()) {
                var ret = queue.PeekFirst();
                Debug.Assert(queue.IsFull);
                yield return (ret, queue.Count);
                queue.Enqueue(enumerator.Current);
            }

        IterateQueue:
            while (queue.TryDequeueFirst(out var item)) {
                yield return (item, queue.Count);
            }
        }
    }

    private sealed class CollectionLookAheadIterator<T>(IEnumerable<T> source, int count, int maxAheadCount) : CollectionIteratorBase<(T, int)>
    {
        private IEnumerator<T> _enumerator = default!;
        private T? _current;
        private int _iteratedCount;

        public override int Count => count;

        public override (T, int) Current => (_current!, Math.Min(maxAheadCount, count - _iteratedCount));

        public override bool MoveNext()
        {
            const int End = MinPreservedState - 1;

            switch (_state) {
                case InitState:
                    _enumerator = source.GetEnumerator();
                    _state = 0;
                    _iteratedCount = 0;
                    goto default;
                case End:
                    return false;
                default:
                    if (_enumerator.MoveNext()) {
                        _current = _enumerator.Current;
                        _iteratedCount++;
                        return true;
                    }
                    else {
                        _state = End;
                        return false;
                    }
            }
        }
        protected override IteratorBase<(T, int)> Clone() => new CollectionLookAheadIterator<T>(source, count, maxAheadCount);
    }

    private sealed class ListLookAheadIterator<T>(IReadOnlyList<T> list, int maxAheadCount) : ListIteratorBase<(T, int)>
    {
        private (T, int) _current;

        public override (T, int) this[int index] => (list[index], Math.Min(maxAheadCount, list.Count - index - 1));

        public override int Count => list.Count;

        public override (T, int) Current => _current;

        public override bool MoveNext() => MoveNext_Index(ref _current);

        protected override IteratorBase<(T, int)> Clone() => new ListLookAheadIterator<T>(list, maxAheadCount);
    }
}
