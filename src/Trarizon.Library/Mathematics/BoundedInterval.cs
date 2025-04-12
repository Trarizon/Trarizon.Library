using CommunityToolkit.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace Trarizon.Library.Mathematics;
public struct BoundedInterval : IEquatable<BoundedInterval>
#if NET7_0_OR_GREATER
    , IEqualityOperators<BoundedInterval, BoundedInterval, bool>
#endif
{
    public float Start;
    public float End;
    public bool IsClosedStart;
    public bool IsClosedEnd;

    public static BoundedInterval Empty => default!;

    public readonly float Length => End - Start;

    public readonly bool IsEmpty => Start == End && !(IsClosedStart && IsClosedEnd);

    public BoundedInterval(float start, bool closedStart, float end, bool closedEnd)
    {
        if (start > end)
            ThrowHelper.ThrowArgumentException("Start of interval should less than or equals to end");
        Start = start;
        End = end;
        IsClosedStart = closedStart;
        IsClosedEnd = closedEnd;
    }

    #region Math operators

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

    public static (BoundedInterval Left, BoundedInterval Right) Substract(BoundedInterval left, BoundedInterval right)
    {
        if (right.End < left.Start)
            return (left, Empty);

        if (right.Start > left.End)
            return (left, Empty);

        if (right.IsEmpty)
            return (left, Empty);

        BoundedInterval rtnl;
        if (right.Start < left.Start)
            rtnl = Empty;
        else
            rtnl = new BoundedInterval(left.Start, left.IsClosedStart, right.Start, !right.IsClosedStart);

        BoundedInterval rtnr;
        if (right.End > left.End)
            rtnr = Empty;
        else
            rtnr = new BoundedInterval(right.End, !right.IsClosedEnd, left.End, left.IsClosedEnd);

        return (rtnl, rtnr);
    }

    public static (BoundedInterval Left, BoundedInterval Right) Union(BoundedInterval left, BoundedInterval right)
    {
        if (right.End < left.Start)
            return (right, left);
        if (right.End == left.Start && !right.IsClosedEnd && !left.IsClosedStart)
            return (right, left);

        if (left.End < right.Start)
            return (left, right);
        if (left.End == right.Start && !left.IsClosedEnd && !right.IsClosedStart)
            return (left, right);

        (float, bool) start;
        if (right.Start < left.Start)
            start = (right.Start, right.IsClosedStart);
        else if (right.Start == left.Start)
            start = (right.Start, right.IsClosedStart || left.IsClosedStart);
        else
            start = (left.Start, left.IsClosedStart);

        (float, bool) end;
        if (right.End > left.End)
            end = (right.End, right.IsClosedEnd);
        else if (right.End == left.End)
            end = (right.End, right.IsClosedEnd || left.IsClosedEnd);
        else
            end = (left.End, left.IsClosedEnd);

        return (new(start.Item1, start.Item2, end.Item1, end.Item2), Empty);
    }

    #endregion

    public readonly bool Contains(float value)
        => (IsClosedStart ? value >= Start : value > Start) && (IsClosedEnd ? value <= End : value < End);

    public static implicit operator BoundedInterval(Interval interval)
        => new(interval.Start, true, interval.End, false);

    #region Equality

    public static bool operator ==(BoundedInterval left, BoundedInterval right)
        => left.Start == right.Start && left.End == right.End
        && left.IsClosedStart == right.IsClosedStart && left.IsClosedEnd == right.IsClosedEnd;
    public static bool operator !=(BoundedInterval left, BoundedInterval right) => !(left == right);

    public readonly bool Equals(BoundedInterval other) => this == other;

    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is BoundedInterval val && Equals(val);

    public override readonly int GetHashCode() => HashCode.Combine(Start, End, IsClosedStart, IsClosedEnd);

    #endregion

    public readonly void Deconstruct(out float start, out bool isClosedStart, out float end, out bool isClosedEnd)
        => (start, isClosedStart, end, isClosedEnd) = (Start, IsClosedStart, End, IsClosedEnd);

    public override readonly string ToString() => $"{(IsClosedStart ? '[' : '(')}{Start}, {End}{(IsClosedEnd ? ']' : ')')}";

    public readonly string ToString(string? format) => $"{(IsClosedStart ? '[' : '(')}{Start.ToString(format)}, {End.ToString(format)}{(IsClosedEnd ? ']' : ')')}";
}
