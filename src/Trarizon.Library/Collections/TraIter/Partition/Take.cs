using System.ComponentModel;

namespace Trarizon.Library.Collections;
partial class TraIter
{
    public static TakeIterator<T> IterTake<T>(this IEnumerable<T> source, int count)
        => new(source, count);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct TakeIterator<T>(IEnumerable<T> collection, int count) : IDisposable
    {
        private readonly int _count = count;
        private IEnumerator<T> _enumerator = default!;
        private int _index = default!;

        public TakeIterator<T> GetEnumerator() => this with { _enumerator = collection.GetEnumerator(), _index = 0 };

        public T Current { get; private set; } = default!;

        public bool MoveNext()
        {
            if (_index >= _count)
                return false;

            if (_enumerator.MoveNext()) {
                Current = _enumerator.Current;
                _index++;
                return true;
            }
            else {
                Current = default!;
                return false;
            }
        }

        public void Dispose()
        {
            _enumerator?.Dispose();
            _enumerator = null!;
        }
    }
}
