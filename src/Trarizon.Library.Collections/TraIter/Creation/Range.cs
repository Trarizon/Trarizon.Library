using System.ComponentModel;

namespace Trarizon.Library.Collections;
public static partial class TraIter
{
    /// <summary>
    /// Enumerate from <paramref name="start"/> to <paramref name="end"/>,
    /// does not include <paramref name="end"/>
    /// </summary>
    public static RangeIterator Range(int start, int end)
        => new(start, end, start <= end ? 1 : -1);

    /// <summary>
    /// Enumerate from 0 to <paramref name="count"/>,
    /// does not include <paramref name="count"/>
    /// </summary>
    public static RangeIterator RangeTo(int count, int step = 1)
        => new(0, count, step);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct RangeIterator(int start, int end, int step)
    {
        private readonly int _end = end;
        private readonly int _step = step;
        private int _current = start;

        public RangeIterator GetEnumerator() => this with { _current = _current - _step };

        public readonly int Current => _current;

        public bool MoveNext()
        {
            if (_current == _end)
                return false;

            _current += _step;
            return _current != _end;
        }
    }
}
