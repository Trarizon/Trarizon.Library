﻿using CommunityToolkit.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

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

    public readonly Interval Intersect(Interval other)
    {
        if (End <= other.Start || Start >= other.End)
            return Empty;
        return new Interval(float.Max(Start, other.Start), float.Min(End, other.End));
    }

    public static bool operator ==(Interval left, Interval right)
        => left.Start == right.Start && left.End == right.End;
    public static bool operator !=(Interval left, Interval right) => !(left == right);

    public readonly bool Equals(Interval other) => this == other;

    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is Interval val && Equals(val);

    public override readonly int GetHashCode() => HashCode.Combine(Start, End);

    public override readonly string ToString() => $"[{Start}, {End})";
}