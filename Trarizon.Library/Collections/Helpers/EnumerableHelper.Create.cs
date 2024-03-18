using System.Collections;
using System.ComponentModel;
using Trarizon.Library.Wrappers;

namespace Trarizon.Library.Collections.Helpers;
partial class EnumerableHelper
{
    public static SelectNextUntilEnumerable<T> SelectNextUntil<T>(T first, Func<T, Optional<T>> nextWhereSelector)
        => new(first, nextWhereSelector);

    public static SelectNextUntilNullEnumerable<T> SelectNextUntilNull<T>(T first, Func<T, T?> nextWhereSeletor) where T : class
        => new(first, nextWhereSeletor);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public readonly struct SelectNextUntilEnumerable<T>(T first, Func<T, Optional<T>> whereSelector) : IEnumerable<T>
    {
        private readonly T _first = first;
        private readonly Func<T, Optional<T>> _whereSelector = whereSelector;

        public Enumerator GetEnumerator() => new(this);

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => new Enumerator.Wrapper(this);
        IEnumerator IEnumerable.GetEnumerator() => new Enumerator.Wrapper(this);

        public struct Enumerator(in SelectNextUntilEnumerable<T> enumerable)
        {
            private int _state = 0;
            private T _current = enumerable._first;
            private readonly Func<T, Optional<T>> _selector = enumerable._whereSelector;

            public readonly T Current => _state == 1 ? _current : default!;

            public bool MoveNext()
            {
                switch (_state) {
                    case 0:
                        _state = 1;
                        return true;
                    case 1: {
                        var cur = _selector(_current);
                        if (cur.HasValue) {
                            _current = cur.Value;
                            return true;
                        }
                        else {
                            _state = 2;
                            return false;
                        }
                    }
                    default:
                        return false;
                }
            }

            internal sealed class Wrapper(in SelectNextUntilEnumerable<T> enumerable) : IEnumerator<T>
            {
                private readonly T _first = enumerable._first;
                private Enumerator _enumerator = enumerable.GetEnumerator();

                public T Current => _enumerator.Current;

                public void Dispose() { }
                public bool MoveNext() => _enumerator.MoveNext();
                public void Reset()
                {
                    _enumerator._state = 0;
                    _enumerator._current = _first;
                }

                object IEnumerator.Current => Current!;
            }
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public readonly struct SelectNextUntilNullEnumerable<T>(T first, Func<T, T?> whereSelector) : IEnumerable<T> where T : class
    {
        private readonly T _first = first;
        private readonly Func<T, T?> _whereSelector = whereSelector;

        public Enumerator GetEnumerator() => new(this);

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => new Enumerator.Wrapper(this);
        IEnumerator IEnumerable.GetEnumerator() => new Enumerator.Wrapper(this);

        public struct Enumerator(in SelectNextUntilNullEnumerable<T> enumerable)
        {
            private int _state = 0;
            private T _current = enumerable._first;
            private readonly Func<T, T?> _selector = enumerable._whereSelector;

            public readonly T Current => _state == 1 ? _current : default!;

            public bool MoveNext()
            {
                switch (_state) {
                    case 0:
                        _state = 1;
                        return true;
                    case 1: {
                        var cur = _selector(_current);
                        if (cur is not null) {
                            _current = cur;
                            return true;
                        }
                        else {
                            _state = 2;
                            return false;
                        }
                    }
                    default:
                        return false;
                }
            }

            internal sealed class Wrapper(in SelectNextUntilNullEnumerable<T> enumerable) : IEnumerator<T>
            {
                private readonly T _first = enumerable._first;
                private Enumerator _enumerator = enumerable.GetEnumerator();

                public T Current => _enumerator.Current;

                public void Dispose() { }
                public bool MoveNext() => _enumerator.MoveNext();
                public void Reset()
                {
                    _enumerator._state = 0;
                    _enumerator._current = _first;
                }

                object IEnumerator.Current => Current!;
            }
        }
    }
}
