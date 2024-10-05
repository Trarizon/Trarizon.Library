using System.Collections;
using System.ComponentModel;

namespace Trarizon.Library.Collections;
partial class TraIter
{
    public static OfTypeIterator<T> IterOfType<T>(this IEnumerable source)
        => new(source);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct OfTypeIterator<T>(IEnumerable source) : IDisposable
    {
        private IEnumerator _enumerator;
        private int _state;

        public OfTypeIterator<T> GetEnumerator() => this with
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
                if (_enumerator.Current is T curr) {
                    Current = curr;
                    return true;
                }
            }

            _state = 0;
            return false;
        }

        public void Dispose()
        {
            (_enumerator as IDisposable)?.Dispose();
            _enumerator = null!;
        }
    }
}
