using CommunityToolkit.Diagnostics;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Trarizon.Library.Numerics;
/// <summary>
/// Left open, right closed interval, in <see cref="float"/>
/// </summary>
public struct Interval : IEquatable<Interval>, IEqualityOperators<Interval, Interval, bool>
{
    public float Start;
    public float End;

    public static Interval Empty => default;

    public Interval(float start, float end)
    {
        if (start > end)
            ThrowHelper.ThrowArgumentException("Start of interval should less than or equals to end");
        Start = start;
        End = end;
    }

    public readonly float Length => End - Start;

    public readonly bool IsEmpty => Length == 0;

    public static Interval Intersect(Interval left, Interval right)
    {
        if (left.End <= right.Start || left.Start >= right.End)
            return Empty;
        return new Interval(float.Max(left.Start, right.Start), float.Min(left.End, right.End));
    }

    public readonly Interval Intersect(Interval other) => Intersect(this, other);

    /// <returns>
    /// If the result is empty, both items are empty
    /// If the result is one interval, the second item is empty,
    /// </returns>
    public static (Interval Left, Interval Right) Substract(Interval left, Interval right)
    {
        if (right.Start <= left.Start) {
            if (right.End <= left.Start)
                return (left, Empty);
            if (right.End < left.End)
                return (new Interval(right.End, left.End), Empty);
            Debug.Assert(right.End >= left.End);
            return (Empty, Empty);
        }
        var rtnL = new Interval(left.Start, right.Start);
        if (right.Start <= left.End) {
            if (right.End < left.End)
                return (rtnL, new Interval(right.End, left.End));
            else
                return (rtnL, Empty);
        }
        Debug.Assert(right.Start >= left.End);
        return (left, Empty);
    }

    public static bool operator ==(Interval left, Interval right)
        => left.Start == right.Start && left.End == right.End;
    public static bool operator !=(Interval left, Interval right) => !(left == right);

    public readonly bool Equals(Interval other) => this == other;

    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is Interval val && Equals(val);

    public override readonly int GetHashCode() => HashCode.Combine(Start, End);

    public override readonly string ToString() => $"[{Start}, {End})";
}
