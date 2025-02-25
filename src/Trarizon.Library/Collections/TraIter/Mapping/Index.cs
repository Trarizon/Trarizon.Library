using System.ComponentModel;

namespace Trarizon.Library.Collections;
public static partial class TraIter
{
    public static WithIndexIterator<T> IterWithIndex<T>(this IEnumerable<T> source)
        => new(source);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct WithIndexIterator<T>(IEnumerable<T> values)
    {
        private int _index;
        private T? _current;
        private IEnumerator<T> _enumerator = default!;

        public readonly (int Index, T Value) Current => (_index, _current!);

        public WithIndexIterator<T> GetEnumerator() => this with
        {
            _index = -1,
            _enumerator = values.GetEnumerator(),
        };

        public bool MoveNext()
        {
            if (!_enumerator.MoveNext())
                return false;
            _index++;
            _current = _enumerator.Current;
            return true;
        }
    }
}