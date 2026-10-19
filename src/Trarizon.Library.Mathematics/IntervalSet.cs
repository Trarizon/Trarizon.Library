using System.Collections;
using System.Numerics;

namespace Trarizon.Library.Mathematics;

#if NET7_0_OR_GREATER

public interface IIntervalSet<T> : IReadOnlyList<Interval<T>> where T : INumber<T>
{
    bool Contains(T value);
}

public readonly struct IntervalSet<T> : IIntervalSet<T>
    where T : INumber<T>
{
    private readonly Interval<T>[] _intervals;
    private readonly int _length;

    public Interval<T> this[int index] => _intervals[index];

    public int Count => _length;

    private IntervalSet(Interval<T>[] intervals)
    {
        _intervals = intervals;
        _length = intervals.Length;
    }

    public bool Contains(T value)
    {
        foreach (var interval in _intervals)
        {
            if (interval.Contains(value))
                return true;
        }
        return false;
    }

    public static IntervalSet<T> Create(Interval<T>[] intervals)
    {
        return new(intervals);
    }


#region Enumerator

    public Enumerator<IntervalSet<T>> GetEnumerator() => new(this);
    IEnumerator<Interval<T>> IEnumerable<Interval<T>>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

#endregion

    public struct Enumerator<TGroup> : IEnumerator<Interval<T>> where TGroup : IIntervalSet<T>
    {
        private readonly TGroup _group;
        private int _index;

        internal Enumerator(TGroup group)
        {
            _group = group;
            _index = -1;
        }

        public readonly Interval<T> Current => _group[_index];

        readonly object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            var index = _index + 1;
            if (index < _group.Count)
            {
                _index = index;
                return true;
            }
            return false;
        }
        public void Reset() => _index = -1;
        readonly void IDisposable.Dispose() { }
    }
}

#endif
