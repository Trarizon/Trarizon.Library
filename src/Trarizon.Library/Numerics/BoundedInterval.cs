using CommunityToolkit.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace Trarizon.Library.Numerics;
public struct BoundedInterval : IEquatable<BoundedInterval>, IEqualityOperators<BoundedInterval, BoundedInterval, bool>
{
    public float Start;
    public float End;
    public bool IsClosedStart;
    public bool IsClosedEnd;

    public static BoundedInterval Empty => default!;

    public readonly float Length => End - Start;

    public BoundedInterval(float start, bool closedStart, float end, bool closedEnd)
    {
        if (start > end)
            ThrowHelper.ThrowArgumentException("Start of interval should less than or equals to end");
        Start = start;
        End = end;
        IsClosedStart = closedStart;
        IsClosedEnd = closedEnd;
    }

    public static BoundedInterval Intersect(BoundedInterval left, BoundedInterval right)
    {
        if (left.End < right.Start || left.Start < right.End)
            return Empty;

        var (start, closedS) = left.Start > right.Start ? (left.Start, left.IsClosedStart)
            : left.Start == right.Start ? (left.Start, left.IsClosedStart && right.IsClosedStart)
            : (right.Start, right.IsClosedStart);

        var (end, closede) = left.End > right.End ? (left.End, left.IsClosedEnd)
            : left.End == right.End ? (left.End, left.IsClosedEnd && right.IsClosedEnd)
            : (right.End, right.IsClosedEnd);

        return new BoundedInterval(start, closedS, end, closede);
    }

    public readonly BoundedInterval Intersect(BoundedInterval other) => Intersect(this, other);

    public static implicit operator BoundedInterval(Interval interval)
        => new(interval.Start, true, interval.End, false);

    public static bool operator ==(BoundedInterval left, BoundedInterval right)
        => left.Start == right.Start && left.End == right.End
        && left.IsClosedStart == right.IsClosedStart && left.IsClosedEnd == right.IsClosedEnd;
    public static bool operator !=(BoundedInterval left, BoundedInterval right) => !(left == right);

    public readonly bool Equals(BoundedInterval other) => this == other;

    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is BoundedInterval val && Equals(val);

    public override readonly int GetHashCode() => HashCode.Combine(Start, End, IsClosedStart, IsClosedEnd);

    public override readonly string ToString() => $"{(IsClosedStart ? '[' : '(')}{Start}, {End}{(IsClosedEnd ? ']' : ')')}";
}
