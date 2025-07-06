using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Trarizon.Library.Mathematics.Helpers;

namespace Trarizon.Library.Mathematics;
/// <summary>
/// Left close, right open interval, in <see cref="float"/>
/// </summary>
public struct Interval : IEquatable<Interval>
#if NET7_0_OR_GREATER
    , IEqualityOperators<Interval, Interval, bool>
#endif
{
    public float Start;
    public float End;

    public static Interval Empty => default;

    public Interval(float start, float end)
    {
        if (start > end)
            Throws.ThrowArgument("Start of interval should less than or equals to end");
        Start = start;
        End = end;
    }

    public readonly float Length => End - Start;

    public readonly bool IsEmpty => Start == End;

    #region Math operators

    public static Interval Intersect(Interval left, Interval right)
    {
        if (left.End <= right.Start || left.Start >= right.End)
            return Empty;
        return new Interval(Math.Max(left.Start, right.Start), Math.Min(left.End, right.End));
    }

    /// <returns>
    /// If the result is empty, both items are empty
    /// If the result is one interval, the second item is empty,
    /// </returns>
    public static (Interval Left, Interval Right) Substract(Interval left, Interval right)
    {
        if (right.End < left.Start)
            return (left, Empty);
        if (right.Start > left.End)
            return (left, Empty);

        if (right.IsEmpty)
            return (left, Empty);

        Interval rtnl;
        if (right.Start < left.Start)
            rtnl = Empty;
        else
            rtnl = new Interval(left.Start, right.Start);

        Interval rtnr;
        if (right.End > left.End)
            rtnr = Empty;
        else
            rtnr = new Interval(right.End, left.End);

        return (rtnl, rtnr);
    }

    public static (Interval Left, Interval Right) Union(Interval left, Interval right)
    {
        if (right.End < left.Start)
            return (right, left);

        if (right.Start > left.End)
            return (left, right);

        return (new Interval(Math.Min(left.Start, right.Start), Math.Max(left.End, right.End)), Empty);
    }

#endregion

    public readonly bool Contains(float value) => value >= Start && value < End;

    #region Equality

    public static bool operator ==(Interval left, Interval right)
        => left.Start == right.Start && left.End == right.End;
    public static bool operator !=(Interval left, Interval right) => !(left == right);

    public readonly bool Equals(Interval other) => this == other;

    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is Interval val && Equals(val);

    public override readonly int GetHashCode() => HashCode.Combine(Start, End);

    #endregion

    public readonly void Deconstruct(out float start, out float end) => (start, end) = (Start, End);

    public override readonly string ToString() => $"[{Start}, {End})";

    public readonly string ToString(string? format) => $"[{Start.ToString(format)}, {End.ToString(format)})";
}
