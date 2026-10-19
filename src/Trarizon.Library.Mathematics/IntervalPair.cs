using System.Collections;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Trarizon.Library.Mathematics;

#if NET7_0_OR_GREATER

[StructLayout(LayoutKind.Sequential)]
public readonly struct IntervalPair<T>
    : IIntervalSet<T>
    where T : INumber<T>
{
    private readonly Interval<T> _first;
    private readonly Interval<T> _second;

    public ReadOnlySpan<Interval<T>> Intervals => MemoryMarshal.CreateReadOnlySpan(in _first, Count);

    public Interval<T> this[int index] => Intervals[index];

    public int Count => _second.IsEmpty ? 1 : 2;

    internal IntervalPair(Interval<T> first, Interval<T> second)
    {
        Debug.Assert(Interval<T>.IsDisjoint(first, second));
        Debug.Assert(first.Lower < second.Lower); // Keep in order
        Debug.Assert((!first.IsUpperInclusive && !second.IsLowerInclusive) || (first.Upper < second.Lower)); // Not continuous
        _first = first;
        _second = second;
    }

    public bool Contains(T value)
    {
        if (_first.Contains(value))
            return true;
        if (_second == default)
            return false;
        return _second.Contains(value);
    }

    #region Enumerator

    public IntervalSet<T>.Enumerator<IntervalPair<T>> GetEnumerator() => new(this);
    IEnumerator<Interval<T>> IEnumerable<Interval<T>>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion
}

#endif
