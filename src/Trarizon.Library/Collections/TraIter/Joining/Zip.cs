using System.ComponentModel;

namespace Trarizon.Library.Collections;
partial class TraIter
{
    public static ZipIterator<T, T2> IterZip<T, T2>(this IEnumerable<T> source, IEnumerable<T2> another)
        => new(source, another);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct ZipIterator<T, T2>(IEnumerable<T> source, IEnumerable<T2> other) : IDisposable
    {
        private IEnumerator<T> _enumerator1;
        private IEnumerator<T2> _enumerator2;
        private int _state;

        public ZipIterator<T, T2> GetEnumerator() => this with
        {
            _enumerator1 = source.GetEnumerator(),
            _enumerator2 = other.GetEnumerator(),
            _state = -1
        };

        public (T, T2) Current { get; private set; } = default;

        public bool MoveNext()
        {
            if (_state == 0)
                return false;

            if (_enumerator1.MoveNext() && _enumerator2.MoveNext()) {
                Current = (_enumerator1.Current, _enumerator2.Current);
                return true;
            }

            _state = 0;
            return false;
        }

        public void Dispose()
        {
            _enumerator1?.Dispose();
            _enumerator2?.Dispose();
            _enumerator1 = null!;
            _enumerator2 = null!;
        }
    }
}
