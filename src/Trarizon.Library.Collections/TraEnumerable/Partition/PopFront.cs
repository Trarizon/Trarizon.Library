using System.Diagnostics;
using Trarizon.Library.Collections.Helpers;

namespace Trarizon.Library.Collections;
public static partial class TraEnumerable
{
    public static IEnumerable<T> PopFront<T>(this IEnumerable<T> source, int count, out IEnumerable<T> leading)
    {
        var front = new PopFrontIterator<T>(source, count);
        var rest = front.GetRestCollection();
        leading = front;
        return rest;
    }

    public static IEnumerable<T> PopFirst<T>(this IEnumerable<T> source, out T first)
    {
        if (source.TryGetNonEnumeratedCount(out int count) && count < 1) {
            Throws.CollectionHasNoElement();
            first = default!;
            return default!;
        }

        var enumerator = source.GetEnumerator();
        if (!enumerator.MoveNext()) {
            enumerator.Dispose();
            first = default!;
            return [];
        }

        first = enumerator.Current;
        return new PopFirstIterator<T>(source, enumerator);
    }

    private sealed class PopFrontIterator<T>(IEnumerable<T> source, int count) : IteratorBase<T>
    {
        private IEnumerator<T> _enumerator = default!;
        private T[] _cachedItems = default!;
        private int _cachedCount;
        private T _current = default!;

        private RestCollection? _associatedRestCollection;

        public override T Current => _current;

        public override bool MoveNext()
        {
            const int End = MinPreservedState - 1;
            const int AllCached = End - 1;

            switch (_state) {
                case InitState:
                    if (_enumerator is null) {
                        _enumerator ??= source.GetEnumerator();
                        _cachedItems ??= new T[count];
                        _state = 0;
                        goto default;
                    }
                    else {
                        // The value may be assigned in IterateToCacheAll when _state is -2
                        _state = AllCached;
                        goto Case_AllCached;
                    }
                case End:
                    return false;
                case <= AllCached:
                Case_AllCached:
                    var index = AllCached - _state;
                    if (index < _cachedCount) {
                        _current = _cachedItems[index];
                        _state--;
                        return true;
                    }
                    else {
                        _current = default!;
                        _state = End;
                        return false;
                    }
                default:
                    Debug.Assert(_state >= 0);
                    if (_state < _cachedCount) {
                        _current = _cachedItems[_state];
                        _state++;
                        return true;
                    }

                    Debug.Assert(_state == _cachedCount);
                    if (_cachedCount >= _cachedItems.Length) {
                        _current = default!;
                        _state = End;
                        return false;
                    }

                    if (_enumerator.MoveNext()) {
                        _current = _enumerator.Current;
                        _cachedItems[_cachedCount] = _current;
                        _state++;
                        _cachedCount++;
                        return true;
                    }
                    else {
                        _state = End;
                        _current = default!;
                        return false;
                    }
            }
        }

        private void IterateToCacheAll()
        {
            const int End = MinPreservedState - 1;
            const int AllCached = End - 1;

            switch (_state) {
                case InitState:
                    _enumerator = source.GetEnumerator();
                    _cachedItems = new T[count];
                    goto default;
                case <= AllCached:
                    return;
                case MinPreservedState: // -2, the collection has never used
                    _enumerator = source.GetEnumerator();
                    _cachedItems = new T[count];
                    for (; _cachedCount < _cachedItems.Length; _cachedCount++) {
                        if (_enumerator.MoveNext())
                            _cachedItems[_cachedCount] = _enumerator.Current;
                        else
                            break;
                    }
                    // Do not reset state
                    break;
                default:
                    for (; _cachedCount < _cachedItems.Length; _cachedCount++) {
                        if (_enumerator.MoveNext())
                            _cachedItems[_cachedCount] = _enumerator.Current;
                        else
                            break;
                    }
                    _state = AllCached - _state;
                    break;
            }
        }

        public RestCollection GetRestCollection()
        {
            const int End = MinPreservedState - 1;
            const int AllCached = End - 1;

            if (_state <= AllCached || _state == End) {
                // If items of this LeadingIterator has all cached, we create a 
                // new enumerator for the new rest collection.
                // Otherwise, just use this._enumerator
                var newEnumerator = source.GetEnumerator();
                for (int i = 0; i < _cachedCount; i++) {
                    newEnumerator.MoveNext();
                }
                return new RestCollection(this, newEnumerator);
            }
            else {
                _associatedRestCollection = new RestCollection(this);
                return _associatedRestCollection;
            }
        }

        protected override IteratorBase<T> Clone() => new PopFrontIterator<T>(source, count);

        protected override void DisposeInternal()
        {
            const int End = MinPreservedState - 1;
            _current = default!;
            _state = End;

            if (_associatedRestCollection is not null) {
                if (_associatedRestCollection.State is End) {
                    _enumerator?.Dispose();
                    _enumerator = null!;
                }
                else {
                    // Do not dispose;
                }
            }
            else {
                _enumerator?.Dispose();
                _enumerator = null!;
            }
        }

        public sealed class RestCollection(PopFrontIterator<T> iterator, IEnumerator<T>? enumerator = null!) : IteratorBase<T>
        {
            private readonly PopFrontIterator<T> _iterator = iterator;
            private IEnumerator<T> _enumerator = enumerator!;
            private T _current = default!;

            public int State => _state;

            public override T Current => _current;

            public override bool MoveNext()
            {
                const int End = MinPreservedState - 1;

                switch (_state) {
                    case InitState:
                        _iterator.IterateToCacheAll();
                        _enumerator ??= _iterator._enumerator;
                        _state = 0;
                        goto default;
                    case End:
                        return false;
                    default:
                        if (_enumerator.MoveNext()) {
                            _current = _enumerator.Current;
                            return true;
                        }
                        else {
                            _current = default!;
                            _state = End;
                            return false;
                        }
                }
            }

            protected override IteratorBase<T> Clone() => _iterator.GetRestCollection();

            protected override void DisposeInternal()
            {
                const int End = MinPreservedState - 1;
                const int AllCached = End - 1;
                _current = default!;
                _state = End;

                if (_state == InitState) {
                    // Here if _enumerator is not null, then the _enumerator is cloned
                    if (_enumerator is not null) {
                        _enumerator.Dispose();
                        _enumerator = null!;
                    }
                    // Here the _enumerator is associated to PopFrontIterator
                    else {
                        if (_iterator._state is <= AllCached or End) {
                            _iterator._enumerator.Dispose();
                            _iterator._enumerator = null!;
                        }
                        else {
                            // Here PopFrontIterator will dispose.
                        }
                    }
                }
                else if (_enumerator == _iterator._enumerator) {
                    // Here PopFrontIterator and RestCollection use the same 
                    // enumerator
                    if (_iterator._state is End or <= AllCached) {
                        // LeadingElements cached, safe to dispose
                        _enumerator?.Dispose();
                        _enumerator = null!;
                    }
                    else {
                        Debug.Assert(_iterator._state is MinPreservedState or InitState);
                        // This branch never reached.
                    }
                }
                else {
                    // Here RestCollection is cloned, thus use different 
                    // enumerator, we should dispose it
                    _enumerator?.Dispose();
                    _enumerator = null!;
                }
            }
        }
    }

    private sealed class PopFirstIterator<T>(IEnumerable<T> source, IEnumerator<T>? enumerator = null) : IteratorBase<T>
    {
        private IEnumerator<T> _enumerator = enumerator!;
        private T _current = default!;

        public override T Current => _current;

        public override bool MoveNext()
        {
            const int End = MinPreservedState - 1;

            switch (_state) {
                case InitState:
                    if (_enumerator is null) {
                        _enumerator = source.GetEnumerator();
                        _enumerator.MoveNext();
                    }
                    _state = 0;
                    goto default;
                case End:
                    return false;
                default:
                    if (_enumerator.MoveNext()) {
                        _current = _enumerator.Current;
                        return true;
                    }
                    else {
                        _current = default!;
                        _state = End;
                        return false;
                    }
            }
        }

        protected override IteratorBase<T> Clone() => new PopFirstIterator<T>(source);

        protected override void DisposeInternal()
        {
            const int End = MinPreservedState - 1;
            _current = default!;
            _state = End;
            _enumerator?.Dispose();
            _enumerator = null!;
        }
    }
}
