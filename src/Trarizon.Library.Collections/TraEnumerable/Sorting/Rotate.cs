using System.Diagnostics;
using Trarizon.Library.Collections.AllocOpt;
using Trarizon.Library.Collections.Helpers;

namespace Trarizon.Library.Collections;
public static partial class TraEnumerable
{
    public static IEnumerable<T> Rotate<T>(this IEnumerable<T> source, int splitPosition)
    {
        if (splitPosition == 0)
            return source;

        if (source.TryGetNonEnumeratedCount(out var count)) {
            if (splitPosition >= count)
                return source;
            return new CollectionRotateIterator<T>(source, splitPosition, count);
        }

        return Iterate(source, splitPosition);

        static IEnumerable<T> Iterate(IEnumerable<T> source, int splitPosition)
        {
            var firstPart = new AllocOptList<T>();

            using var enumerator = source.GetEnumerator();
            for (int i = 0; i < splitPosition; i++) {
                if (enumerator.MoveNext()) {
                    firstPart.Add(enumerator.Current);
                }
                else {
                    goto YieldFirstPart;
                }
            }

            while (enumerator.MoveNext()) {
                yield return enumerator.Current;
            }

        YieldFirstPart:
            foreach (var item in firstPart)
                yield return item;
        }
    }

    public static IEnumerable<T> Rotate<T>(this IEnumerable<T> source, Index splitPosition)
    {
        if (splitPosition.IsFromEnd) {
            if (source.TryGetNonEnumeratedCount(out var count)) {
                return Rotate(source, splitPosition.GetOffset(count));
            }
            else {
                var cache = new AllocOptList<T>();
                cache.AddRange(source);

                var split = splitPosition.GetOffset(cache.Count);
                if (split < 0)
                    return cache.AsSpan().ToArray();
                return new ListRotateIterator<ArrayTruncation<T>, T>(new(cache.GetUnderlyingArray(), cache.Count), split);
            }
        }
        else {
            return Rotate(source, splitPosition.Value);
        }
    }

    private sealed class CollectionRotateIterator<T>(IEnumerable<T> source, int split, int count) : CollectionIteratorBase<T>
    {
        private List<T> _firstPart = default!;
        private IEnumerator<T> _enumerator = default!;
        private T? _current;

        public override int Count => count;

        public override T Current => _current!;

        public override bool MoveNext()
        {
            const int SecondPart = MinPreservedState - 1;
            const int End = SecondPart - 1;

            switch (_state) {
                case InitState:
                    _enumerator = source.GetEnumerator();
                    _firstPart = new();
                    for (int i = 0; i < split; i++) {
                        if (_enumerator.MoveNext()) {
                            _firstPart.Add(_enumerator.Current);
                        }
                        else {
                            _state = 0;
                            goto default;
                        }
                    }
                    _state = SecondPart;
                    goto case SecondPart;
                case SecondPart:
                    if (_enumerator.MoveNext()) {
                        _current = _enumerator.Current;
                        return true;
                    }
                    else {
                        _state = 0;
                        goto default;
                    }
                case End:
                    return false;
                default:
                    Debug.Assert(_state >= 0);
                    if (_state < _firstPart.Count) {
                        _current = _firstPart[_state];
                        _state++;
                        return true;
                    }
                    else {
                        _state = End;
                        return false;
                    }
            }
        }
        protected override IteratorBase<T> Clone() => new CollectionRotateIterator<T>(source, split, count);

        protected override void DisposeInternal()
        {
            _enumerator.Dispose();
        }
    }

    private sealed class ListRotateIterator<TList, T>(TList list, int split) : ListIteratorBase<T>
        where TList : IReadOnlyList<T>
    {
        private T? _current;

        public override T this[int index]
        {
            get {
                if (index < split)
                    return list[index + split];
                else
                    return list[index - split];
            }
        }

        public override int Count => list.Count;

        public override T Current => _current!;

        public override bool MoveNext()
        {
            const int End = MinPreservedState - 1;
            const int FirstPartStart = End - 1;

            switch (_state) {
                case InitState:
                    _state = 0;
                    goto default;
                case End:
                    return false;
                case <= FirstPartStart: {
                    var index = FirstPartStart - _state;
                    if (index < split) {
                        _current = list[index];
                        _state--;
                        return true;
                    }
                    else {
                        _current = default;
                        _state = End;
                        return false;
                    }
                }
                default: {
                    Debug.Assert(_state >= 0);
                    var index = _state + split;
                    if (index < list.Count) {
                        _current = list[index];
                        _state++;
                        return true;
                    }
                    else {
                        _current = default;
                        _state = FirstPartStart;
                        return MoveNext();
                    }
                }
            }
        }

        protected override IteratorBase<T> Clone() => new ListRotateIterator<TList, T>(list, split);
    }
}
