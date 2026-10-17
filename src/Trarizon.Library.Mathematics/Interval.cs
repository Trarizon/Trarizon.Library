using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Trarizon.Library.Mathematics.Helpers;

namespace Trarizon.Library.Mathematics;

public enum IntervalBoundary : byte
{
    Open = 0,
    ClosedOpen = 1,
    OpenClosed = 2,
    Closed = 3,
}

#if NET7_0_OR_GREATER

public static partial class IntervalExtensions
{
    extension<T>(Interval<T> self) where T : IBinaryInteger<T>
    {
        // 基数，计算总数量的，length不用管开闭但这个要管
        public T Cardinality => self.Boundary switch
        {
            IntervalBoundary.Open => T.Max(T.Zero, self.Upper - self.Lower - T.One),
            IntervalBoundary.ClosedOpen => self.Upper - self.Lower,
            IntervalBoundary.OpenClosed => self.Upper - self.Lower,
            IntervalBoundary.Closed => self.Upper - self.Lower + T.One,
            _ => T.Zero,
        };
    }
}

public struct Interval<T>
    : IEquatable<Interval<T>>
    , ISpanFormattable
    , IUtf8SpanFormattable
    where T : INumber<T>
{
    private const byte LowerInclusive = 1;
    private const byte UpperInclusive = 2;
    private const byte BothInclusive = 3;

    public static Interval<T> Empty => default;

    private T _lower;
    private T _upper;
    private byte _boundary;

    public readonly T Lower => _lower;
    public readonly T Upper => _upper;
    public readonly bool IsLowerInclusive => (_boundary & LowerInclusive) != 0;
    public readonly bool IsUpperInclusive => (_boundary & UpperInclusive) != 0;
    public IntervalBoundary Boundary => (IntervalBoundary)_boundary;

    public readonly T Length => _upper - _lower;

    public readonly bool IsEmpty => _lower == _upper && _boundary != BothInclusive;
    public readonly bool IsDegenerate => _lower == _upper;

    public readonly Bounds<T> Bounds => new(_lower, _upper);

    private Interval(T lower, T upper, byte boundary)
    {
        if (lower > upper)
            Throws.ThrowArgument("Min of bounds should less than or equals to max");
        _lower = lower;
        _upper = upper;
        _boundary = boundary;
    }

    public readonly bool Contains(T value)
        => (IsLowerInclusive ? value >= Lower : value > Lower)
        && (IsUpperInclusive ? value <= Upper : value < Upper);

    public readonly bool Contains(Interval<T> other)
    {
        if (other.IsEmpty)
            return true;
        if (this.IsEmpty)
            return false;

        return (!IsLowerInclusive && other.IsLowerInclusive ? Lower < other.Lower : Lower <= other.Lower)
            && (!IsUpperInclusive && other.IsUpperInclusive ? Upper > other.Upper : Upper >= other.Upper);
    }

    public static bool Overlaps(Interval<T> left, Interval<T> right)
    {
        return (
            left.IsUpperInclusive && right.IsLowerInclusive
                ? left.Upper >= right.Lower
                : left.Upper > right.Lower
        ) && (
            left.IsLowerInclusive && right.IsUpperInclusive
                ? left.Lower <= right.Upper
                : left.Lower < right.Upper
        );
    }

    public static bool IsDisjoint(Interval<T> left, Interval<T> right)
        => !Overlaps(left, right);

    public static Interval<T> Intersect(Interval<T> left, Interval<T> right)
    {
        if (left.IsEmpty || right.IsEmpty)
            return Empty;

        var (lower, lowerBoundary) = left.Lower > right.Lower ? (left.Lower, (byte)(left._boundary & LowerInclusive))
            : left.Lower == right.Lower ? (left.Lower, (byte)(left._boundary & right._boundary & LowerInclusive))
            : (right.Lower, (byte)(right._boundary & LowerInclusive));

        var (upper, upperBoundary) = left.Upper > right.Upper ? (left.Upper, (byte)(left._boundary & UpperInclusive))
            : left.Upper == right.Upper ? (left.Upper, (byte)(left._boundary & right._boundary & UpperInclusive))
            : (right.Upper, (byte)(right._boundary & UpperInclusive));

        return new(lower, upper, (byte)(lowerBoundary | upperBoundary));
    }

    public static (Interval<T>, Interval<T>) Union(Interval<T> left, Interval<T> right)
    {
        return default;
    }

    public static (Interval<T>, Interval<T>) Subtract(Interval<T> left, Interval<T> right)
        => default;

    // 补集
    public static (Interval<T>, Interval<T>) Complement(Interval<T> value)
        => default;

    public Interval<T> WithBoundary(IntervalBoundary boundary)
        => new(_lower, _upper, (byte)boundary);

    public static Interval<T> Open(T lower, T upper) => new(lower, upper, (byte)IntervalBoundary.Open);
    public static Interval<T> Closed(T lower, T upper) => new(lower, upper, (byte)IntervalBoundary.Closed);
    public static Interval<T> OpenClosed(T lower, T upper) => new(lower, upper, (byte)IntervalBoundary.OpenClosed);
    public static Interval<T> ClosedOpen(T lower, T upper) => new(lower, upper, (byte)IntervalBoundary.ClosedOpen);

    #region Equality

    public static bool operator ==(Interval<T> left, Interval<T> right)
        => left._lower == right._lower && left._upper == right._upper && left._boundary == right._boundary;
    public static bool operator !=(Interval<T> left, Interval<T> right) => !(left == right);

    public readonly bool Equals(Interval<T> other) => this == other;
    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is Interval<T> val && Equals(val);

    public override readonly int GetHashCode() => HashCode.Combine(_lower, _upper, _boundary);

    #endregion

    #region String

    public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        if (destination.Length < 6) // [0, 0]
        {
            charsWritten = 0;
            return false;
        }

        if (!Lower.TryFormat(destination[1..], out var lowerWritten, format, provider))
        {
            charsWritten = 0;
            return false;
        }

        if (!Upper.TryFormat(destination[(3 + lowerWritten)..], out var upperWritten, format, provider))
        {
            charsWritten = 0;
            return false;
        }

        destination[0] = IsLowerInclusive ? '[' : '(';
        var idx = lowerWritten + 1;
        destination[idx++] = ',';
        destination[idx++] = ' ';
        destination[idx + upperWritten] = IsUpperInclusive ? ']' : ')';
        charsWritten = lowerWritten + upperWritten + 4;
        return true;
    }

    public readonly bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        if (utf8Destination.Length < 6) // [0, 0]
        {
            bytesWritten = 0;
            return false;
        }

        if (!Lower.TryFormat(utf8Destination[1..], out var lowerWritten, format, provider))
        {
            bytesWritten = 0;
            return false;
        }

        if (!Upper.TryFormat(utf8Destination[(3 + lowerWritten)..], out var upperWritten, format, provider))
        {
            bytesWritten = 0;
            return false;
        }

        utf8Destination[0] = IsLowerInclusive ? (byte)'[' : (byte)'(';
        var idx = lowerWritten + 1;
        utf8Destination[idx++] = (byte)',';
        utf8Destination[idx++] = (byte)' ';
        utf8Destination[idx + upperWritten] = IsUpperInclusive ? (byte)']' : (byte)')';
        bytesWritten = lowerWritten + upperWritten + 4;
        return true;
    }

    public readonly string ToString(string? format, IFormatProvider? formatProvider)
    {
        var left = IsLowerInclusive ? '[' : '(';
        var right = IsUpperInclusive ? ']' : ')';
        return string.Create(formatProvider, stackalloc char[4 + 10], $"{left}{Lower}, {Upper}{right}");
    }

    public readonly string ToString(string? format) => ToString(format, default(IFormatProvider));

    public readonly override string ToString() => ToString(null, null);

    #endregion
}

#endif
