using System.ComponentModel;

namespace Trarizon.Library.Collections;
partial class TraIter
{
    public static OfNotNullReferenceTypeIterator<T> IterOfNotNull<T>(this IEnumerable<T?> source) where T : class
        => new(source);

    public static OfNotNullValueTypeIterator<T> IterOfNotNull<T>(this IEnumerable<T?> source) where T : struct
        => new(source);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct OfNotNullReferenceTypeIterator<T>(IEnumerable<T?> source) : IDisposable where T : class
    {
        private IEnumerator<T?> _enumerator;
        private int _state;

        public OfNotNullReferenceTypeIterator<T> GetEnumerator() => this with
        {
            _enumerator = source.GetEnumerator(),
            _state = -1,
        };

        public T Current { get; private set; } = default!;

        public bool MoveNext()
        {
            if (_state == 0)
                return false;

            while (_enumerator.MoveNext()) {
                if (_enumerator.Current is { } curr) {
                    Current = curr;
                    return true;
                }
            }

            _state = 0;
            return false;
        }

        public void Dispose()
        {
            _enumerator?.Dispose();
            _enumerator = null!;
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct OfNotNullValueTypeIterator<T>(IEnumerable<T?> source) : IDisposable where T : struct
    {
        private IEnumerator<T?> _enumerator;
        private int _state;

        public OfNotNullValueTypeIterator<T> GetEnumerator() => this with
        {
            _enumerator = source.GetEnumerator(),
            _state = -1,
        };

        public T Current { get; private set; } = default!;

        public bool MoveNext()
        {
            if (_state == 0)
                return false;

            while (_enumerator.MoveNext()) {
                if (_enumerator.Current is { } curr) {
                    Current = curr;
                    return true;
                }
            }

            _state = 0;
            return false;
        }

        public void Dispose()
        {
            _enumerator?.Dispose();
            _enumerator = null!;
        }
    }
}
