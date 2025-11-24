using System.Diagnostics;

namespace Trarizon.Library.Linq;

public static partial class TraEnumerable
{
    public static IEnumerable<T> Rotate<T>(this IEnumerable<T> source, int splitPosition)
    {
        if (splitPosition <= 0)
            return source;

        if (source is T[] arr) {
            if (arr.Length <= splitPosition)
                return source;
            return new ListRotateIterator<T>(arr, splitPosition);
        }

        if (source is IList<T> list)
            return new ListRotateIterator<T>(list, splitPosition);

        if (source is ICollection<T> collection)
            return new CollectionRotateIterator<T>(collection, splitPosition);

        return Iterate(source, splitPosition);

        static IEnumerable<T> Iterate(IEnumerable<T> source, int splitPosition)
        {
            var firstPart = new List<T>();

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
        if (!splitPosition.IsFromEnd)
            return Rotate(source, splitPosition.Value);

        var fromEndValue = splitPosition.Value;
        if (fromEndValue <= 0)
            return source;

        if (source is T[] arr) {
            if (arr.Length <= fromEndValue)
                return source;
            return new ListRotateIterator<T>(arr, arr.Length - fromEndValue);
        }

        if (source is IList<T> list) {
            return new ListRotateFromEndIterator<T>(list, fromEndValue);
        }

        return IterateFromEnd(source, fromEndValue);

        static IEnumerable<T> IterateFromEnd(IEnumerable<T> source, int splitFromEnd)
        {
            var cache = source.ToArray();

            if (splitFromEnd < cache.Length) {
                var split = cache.Length - splitFromEnd;
                for (int i = split; i < cache.Length; i++) {
                    yield return cache[i];
                }
                for (int i = 0; i < split; i++) {
                    yield return cache[i];
                }
            }
            else {
                foreach (var item in cache) {
                    yield return item;
                }
            }
        }
    }

    private sealed class CollectionRotateIterator<T>(ICollection<T> source, int split) : CollectionIteratorBase<T>
    {
        private List<T> _firstPart = default!;
        private IEnumerator<T> _enumerator = default!;
        private T? _current;

        public override int Count => source.Count;

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
        protected override IteratorBase<T> Clone() => new CollectionRotateIterator<T>(source, split);

        protected override void DisposeInternal()
        {
            _enumerator.Dispose();
        }
    }

    private sealed class ListRotateIterator<T>(IList<T> list, int split) : ListIteratorBase<T>
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

        protected override IteratorBase<T> Clone() => new ListRotateIterator<T>(list, split);

        internal override T TryGetFirst(out bool exists)
        {
            var count = list.Count;
            if (split < count) {
                exists = true;
                return list[split];
            }
            if (count > 0) {
                exists = true;
                return list[0];
            }
            exists = false;
            return default!;
        }

        internal override T TryGetLast(out bool exists)
        {
            var count = list.Count;
            if (split < count) {
                exists = true;
                return list[split - 1];
            }
            if (count > 0) {
                exists = true;
                return list[^1];
            }
            exists = false;
            return default!;
        }
    }

    private sealed class ListRotateFromEndIterator<T>(IList<T> list, int fromEndSplit) : ListIteratorBase<T>
    {
        private T? _current;

        public override T this[int index]
        {
            get {
                var split = list.Count - fromEndSplit;
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
                    _state = Math.Max(0, fromEndSplit - list.Count);
                    goto default;
                case <= FirstPartStart:
                First:
                    var index = FirstPartStart - _state;
                    if (index < list.Count - fromEndSplit) {
                        _current = list[index];
                        _state--;
                        return true;
                    }
                    else {
                        _current = default;
                        _state = End;
                        return false;
                    }
                default:
                    Debug.Assert(_state >= 0);
                    if (_state < fromEndSplit) {
                        _current = list[list.Count - fromEndSplit + _state];
                        _state++;
                        return true;
                    }
                    else {
                        _current = default;
                        _state = FirstPartStart;
                        goto First;
                    }
            }
        }

        protected override IteratorBase<T> Clone() => new ListRotateFromEndIterator<T>(list, fromEndSplit);

        internal override T TryGetFirst(out bool exists)
        {
            var count = list.Count;
            if (fromEndSplit < list.Count) {
                exists = true;
                return list[^fromEndSplit];
            }
            if (count > 0) {
                exists = true;
                return list[0];
            }
            exists = false;
            return default!;
        }

        internal override T TryGetLast(out bool exists)
        {
            var count = list.Count;
            if (fromEndSplit < count) {
                exists = true;
                return list[^(fromEndSplit + 1)];
            }
            if (count > 0) {
                exists = true;
                return list[^1];
            }
            exists = false;
            return default!;
        }
    }
}
