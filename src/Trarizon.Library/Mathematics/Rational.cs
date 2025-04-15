using CommunityToolkit.Diagnostics;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using Int = int;

namespace Trarizon.Library.Mathematics;
public readonly struct Rational(Int numerator, Int denominator) : IEquatable<Rational>
    , IComparable<Rational>, IComparable
#if NET7_0_OR_GREATER
    , INumber<Rational>
    , ISignedNumber<Rational>
    , IMinMaxValue<Rational>
    , IUtf8SpanParsable<Rational>
#endif
{
    // NaN:  0/0
    // +Inf: +/0
    // -Inf: -/0
    // Number: n/non0

    private readonly Int _numerator = numerator;
    private readonly Int _denominator = denominator;

    public Int Numerator => _numerator;
    public Int Denominator => _denominator;

    public static Rational One => new Rational(1, 1);

    public static Rational Zero => new Rational(0, 1);

    public static Rational PositiveInfinity => new Rational(1, 0);

    public static Rational NegativeInfinity => new Rational(-1, 0);

    public static Rational NaN => new Rational(0, 0);

#if NET7_0_OR_GREATER

    static int INumberBase<Rational>.Radix => 2;

    static Rational IAdditiveIdentity<Rational, Rational>.AdditiveIdentity => Zero;

    static Rational IMultiplicativeIdentity<Rational, Rational>.MultiplicativeIdentity => One;

    public static Rational NegativeOne => new Rational(-1, 1);

    public static Rational MaxValue => new Rational(Int.MaxValue, 1);

    public static Rational MinValue => new Rational(Int.MinValue, 1);

#endif

    #region Operator implementations

    #region Reduction CommonDenominator

    public static Rational Reduce(Rational num)
    {
        switch (num._numerator, num._denominator) {
            case (0, 0): return NaN;
            case ( > 0, 0): return PositiveInfinity;
            case ( < 0, 0): return NegativeInfinity;
            case (0, _): return Zero;
        }

        var gcd = TraMath.GreatestCommonDivisor(num._numerator, num._denominator);
        return new Rational(num._numerator / gcd, num._denominator / gcd);
    }

    /// <summary>
    /// If one of the value is NaN or Infinity, do nothing
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    public static void CommonDenominator(ref Rational left, ref Rational right)
    {
        if (IsFinite(left) && IsFinite(right)) {
            var lcm = TraMath.LeastCommonMultiple(left._denominator, right._denominator);
            left = new Rational(left._numerator * lcm / left._denominator, lcm);
            right = new Rational(right._numerator * lcm / right._denominator, lcm);
        }
    }

    public static (Rational Left, Rational Right) CommonDenominator(Rational left, Rational right)
    {
        if (IsFinite(left) && IsFinite(right)) {
            var lcm = TraMath.LeastCommonMultiple(left._denominator, right._denominator);
            var l = new Rational(left._numerator * lcm / left._denominator, lcm);
            var r = new Rational(right._numerator * lcm / right._denominator, lcm);
            return (l, r);
        }
        else {
            return (left, right);
        }
    }

    private static Int DangerousCommonPositiveDenominator(Rational left, Rational right, out Int numeratorLeft, out Int numeratorRight)
    {
        Debug.Assert(IsFinite(left));
        Debug.Assert(IsFinite(right));

        if (left._denominator == right._denominator) {
            if (left._denominator < 0) {
                numeratorLeft = -left._numerator;
                numeratorRight = -right._numerator;
                return -left._denominator;
            }
            else {
                numeratorLeft = left._numerator;
                numeratorRight = right._numerator;
                return left._denominator;
            }
        }

        var den = left._denominator * right._denominator;
        if (den < 0) {
            numeratorLeft = -left._numerator * right._denominator;
            numeratorRight = -right._numerator * left._denominator;
        }
        else {
            numeratorLeft = left._numerator * right._denominator;
            numeratorRight = right._numerator * left._denominator;
        }
        return den;
    }

    private static Int DangerousCommonDenominator(Rational left, Rational right, out Int numeratorLeft, out Int numeratorRight)
    {
        if (left._denominator == right._denominator) {
            numeratorLeft = left._numerator;
            numeratorRight = left._denominator;
            return left._denominator;
        }

        numeratorLeft = left._numerator * right._denominator;
        numeratorRight = right._numerator * left._denominator;
        var den = left._denominator * right._denominator;
        return den;
    }

    #endregion

    public static Rational Reciprocal(Rational num)
        => new Rational(num._denominator, num._numerator);

    public static Rational Floor(Rational num)
    {
        if (IsFinite(num)) {
            if (IsPositiveOrZeroKnownFinite(num))
                return num._numerator / num._denominator;
            else {
                var quo = Math.DivRem(num._numerator, num._denominator, out var rem);
                if (rem == 0)
                    return quo;
                else
                    return quo - 1;
            }
        }
        else {
            return num;
        }
    }

    public static Rational Ceiling(Rational num)
    {
        if (IsFinite(num)) {
            if (!IsPositiveOrZeroKnownFinite(num))
                return num._numerator / num._denominator;
            else {
                var quo = Math.DivRem(num._numerator, num._denominator, out var rem);
                if (rem == 0)
                    return quo;
                else
                    return quo + 1;
            }
        }
        else {
            return num;
        }
    }

    private static Rational Add(Rational l, Rational r)
    {
        switch (l._numerator, l._denominator, r._numerator, r._denominator) {
            case (_, not 0, _, not 0):
                var den = DangerousCommonDenominator(l, r, out var lnum, out var rnum);
                return new Rational(lnum + rnum, den);
            // NaN
            case (0, 0, _, _) or (_, _, 0, 0): return NaN;
            // +inf + -inf;
            case ( > 0, 0, < 0, 0) or ( < 0, 0, > 0, 0): return NaN;
            // +inf + +inf or n
            case ( > 0, 0, _, _) or (_, _, > 0, 0): return PositiveInfinity;
            // -inf + -inf or n
            case ( < 0, 0, _, _) or (_, _, < 0, 0): return NegativeInfinity;
        }
    }

    private static Rational Multiple(Rational l, Rational r)
    {
        var den = l._denominator * r._denominator;
        var num = l._numerator * r._numerator;
        return new Rational(num, den);
    }

    private static Rational Modulus(Rational left, Rational right)
    {
        if (!IsFinite(left)) {
            return NaN;
        }

        if (right._denominator == 0) {
            if (right._numerator == 0)
                return NaN;
            else
                return left;
        }

        var den = DangerousCommonDenominator(left, right, out var numl, out var numr);
        var mod = numl % numr;
        return new Rational(mod, den);
    }

    private static bool IsGreater(Rational l, Rational r)
    {
        switch (l._numerator, l._denominator, r._numerator, r._denominator) {
            case (_, not 0, _, not 0):
                DangerousCommonPositiveDenominator(l, r, out var numl, out var numr);
                return numl > numr;
            // NaN
            case (0, 0, _, _) or (_, _, 0, 0): return false;
            // Inf
            case ( > 0, 0, > 0, 0): return false; // eq
            case ( < 0, 0, < 0, 0): return false; // eq
            case ( > 0, 0, _, _): return true;
            case (_, _, > 0, 0): return false;
            case ( < 0, 0, _, _): return false;
            case (_, _, < 0, 0): return true;
        }
    }

    private static int Compare(Rational l, Rational r)
    {
        switch (l._numerator, l._denominator, r._numerator, r._denominator) {
            case (_, not 0, _, not 0):
                DangerousCommonPositiveDenominator(l, r, out var numl, out var numr);
                return numl.CompareTo(numr);
            // NaN
            case (0, 0, _, _): return -1;
            case (_, _, 0, 0): return 1;
            // Inf
            case ( > 0, 0, > 0, 0): return 0; // eq
            case ( < 0, 0, < 0, 0): return 0; // eq
            case ( > 0, 0, _, _): return 1;
            case (_, _, > 0, 0): return -1;
            case ( < 0, 0, _, _): return -1;
            case (_, _, < 0, 0): return 1;
        }
    }

    private static bool Equals(Rational l, Rational r)
    {
        return (l._numerator, l._denominator, r._numerator, r._denominator) switch
        {
            (_, not 0, _, not 0) => FiniteImpl(l, r),
            // NaN
            (0, 0, _, _) => false,
            (_, _, 0, 0) => false,
            // Inf
            ( > 0, 0, > 0, 0) or ( < 0, 0, < 0, 0) => true,
            _ => false,
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool FiniteImpl(Rational l, Rational r)
        {
            if (Math.Abs(l._denominator) < Math.Abs(r._denominator)) {
                (l, r) = (r, l);
            }
            var div = Math.DivRem(l._denominator, r._denominator, out var rem);
            if (rem != 0)
                return false;
            return r._numerator * div == l._numerator;
        }
    }

    private static bool IsPositiveOrZeroKnownFinite(Rational value)
    {
        Debug.Assert(IsFinite(value));
        Debug.Assert(value._denominator != 0);
        return (value._numerator >= 0) == (value._denominator > 0);
    }

    #endregion

    public static implicit operator Rational(int n) => new Rational(n, 1);
    public float ToSingle() => (float)_numerator / _denominator;
    public double ToDouble() => (double)_numerator / _denominator;
    public decimal ToDecimal() => (decimal)_numerator / _denominator;

    #region Operators

    public static Rational operator +(Rational left, Rational right)
        => Reduce(Add(left, right));
    public static Rational operator -(Rational left, Rational right)
        => Reduce(Add(left, -right));
    public static Rational operator *(Rational left, Rational right)
        => Reduce(Multiple(left, right));
    public static Rational operator /(Rational left, Rational right)
        => Reduce(Multiple(left, Reciprocal(right)));
    public static Rational operator %(Rational left, Rational right)
        => Reduce(Modulus(left, right));

    public static bool operator ==(Rational left, Rational right) => left.Equals(right);
    public static bool operator !=(Rational left, Rational right) => !left.Equals(right);
    public static bool operator >(Rational left, Rational right) => IsGreater(left, right);
    public static bool operator >=(Rational left, Rational right) => !IsGreater(right, left);
    public static bool operator <(Rational left, Rational right) => IsGreater(right, left);
    public static bool operator <=(Rational left, Rational right) => !IsGreater(left, right);
    public static Rational operator --(Rational value)
        => new Rational(value._numerator - value._denominator, value._denominator);
    public static Rational operator ++(Rational value)
        => new Rational(value._numerator + value._denominator, value._denominator);
    public static Rational operator -(Rational value)
        => new Rational(-value._numerator, value._denominator);
    public static Rational operator +(Rational value)
        => value;

    #endregion

    public bool Equals(Rational other) => Equals(this, other);

    public override bool Equals(object? obj) => obj is Rational number && Equals(number);

    public override int GetHashCode()
    {
        var num = Reduce(this);
        return HashCode.Combine(num._numerator, num._denominator);
    }

    public override string ToString()
    {
        if (_denominator == 0) {
            if (_numerator == 0)
                return "NaN";
            else if (_numerator > 0)
                return "+∞";
            else
                return "-∞";
        }
        else if (_denominator is 1) {
            return _numerator.ToString();
        }
        else {
            var num = Reduce(this);
            return $"{num._numerator}/{num._denominator}";
        }
    }

    int IComparable.CompareTo(object? obj)
    {
        if (obj is not Rational number)
            return ThrowHelper.ThrowInvalidOperationException<int>();
        return Compare(this, number);
    }
    public int CompareTo(Rational other) => Compare(this, other);
    public static Rational Abs(Rational value) => new Rational(Math.Abs(value._numerator), Math.Abs(value._denominator));
    public static bool IsCanonical(Rational value) => TraMath.GreatestCommonDivisor(value._numerator, value._denominator) is 1 or -1;
    public static bool IsComplexNumber(Rational value) => false;
    public static bool IsEvenInteger(Rational value)
    {
        if (!IsFinite(value))
            return false;

        var quo = Math.DivRem(value._numerator, value._denominator, out var rem);
        if (rem != 0)
            return false;
        return quo % 2 == 0;
    }
    public static bool IsFinite(Rational value)
    {
        // 0/0 => false
        // +/0 => false
        // -/0 => false
        return value._denominator is 0;
    }
    public static bool IsImaginaryNumber(Rational value) => false;
    public static bool IsInfinity(Rational value) => value._numerator is not 0 && value._denominator is 0;
    public static bool IsInteger(Rational value) => value._numerator % value._denominator == 0;
    public static bool IsNaN(Rational value) => value._numerator is 0 && value._denominator is 0;
    public static bool IsNegative(Rational value)
    {
        return (value._numerator, value._denominator) switch
        {
            (0, 0) => false,
            // +0       +inf
            (0, > 0) or ( > 0, 0) or ( > 0, > 0) or ( < 0, < 0) => false,
            // -0       -inf
            (0, < 0) or ( < 0, 0) or ( > 0, < 0) or ( < 0, > 0) => true,
        };
    }
    public static bool IsNegativeInfinity(Rational value) => value._numerator < 0 && value._denominator == 0;
    public static bool IsNormal(Rational value)
    {
        // 0/0 +/0 -/0
        if (value._denominator is 0)
            return false;

        // 0/n
        if (value._numerator is 0)
            return false;

        return true;
    }
    public static bool IsOddInteger(Rational value)
    {
        if (!IsFinite(value))
            return false;

        var quo = Math.DivRem(value._numerator, value._denominator, out var rem);
        if (rem != 0)
            return false;
        return quo % 2 != 0;
    }
    public static bool IsPositive(Rational value)
    {
        return (value._numerator, value._denominator) switch
        {
            (0, 0) => false,
            // +0       +inf
            (0, > 0) or ( > 0, 0) or ( > 0, > 0) or ( < 0, < 0) => true,
            // -0       -inf
            (0, < 0) or ( < 0, 0) or ( > 0, < 0) or ( < 0, > 0) => false,
        };
    }
    public static bool IsPositiveInfinity(Rational value) => value._numerator > 0 && value._denominator == 0;
    public static bool IsRealNumber(Rational value) => true;
    public static bool IsSubnormal(Rational value) => false;
    public static bool IsZero(Rational value) => value._numerator is 0 && value._denominator is not 0;
    public static Rational MaxMagnitude(Rational x, Rational y)
    {
        var l = Reduce(Abs(x));
        var r = Reduce(Abs(y));
        if (l > r || IsNaN(x))
            return x;
        if (l == r)
            return IsNegative(x) ? y : x;
        return y;
    }
    public static Rational MaxMagnitudeNumber(Rational x, Rational y)
    {
        var l = Reduce(Abs(x));
        var r = Reduce(Abs(y));

        if (l > r || IsNaN(y))
            return x;
        if (l == r)
            return IsNegative(x) ? y : x;
        return y;
    }
    public static Rational MinMagnitude(Rational x, Rational y)
    {
        var l = Reduce(Abs(x));
        var r = Reduce(Abs(y));
        if (l < r || IsNaN(x))
            return x;
        if (l == r)
            return IsNegative(x) ? x : y;
        return y;
    }
    public static Rational MinMagnitudeNumber(Rational x, Rational y)
    {
        var l = Reduce(Abs(x));
        var r = Reduce(Abs(y));
        if (l < r || IsNaN(y))
            return x;
        if (l == r)
            return IsNegative(x) ? x : y;
        return y;
    }

#if NET7_0_OR_GREATER
    public static bool TryConvertFromChecked<TOther>(TOther value, [MaybeNullWhen(false)] out Rational result) where TOther : INumberBase<TOther> => throw new NotImplementedException();
    public static bool TryConvertFromSaturating<TOther>(TOther value, [MaybeNullWhen(false)] out Rational result) where TOther : INumberBase<TOther> => throw new NotImplementedException();
    public static bool TryConvertFromTruncating<TOther>(TOther value, [MaybeNullWhen(false)] out Rational result) where TOther : INumberBase<TOther> => throw new NotImplementedException();
    public static bool TryConvertToChecked<TOther>(Rational value, [MaybeNullWhen(false)] out TOther result) where TOther : INumberBase<TOther> => throw new NotImplementedException();
    public static bool TryConvertToSaturating<TOther>(Rational value, [MaybeNullWhen(false)] out TOther result) where TOther : INumberBase<TOther> => throw new NotImplementedException();
    public static bool TryConvertToTruncating<TOther>(Rational value, [MaybeNullWhen(false)] out TOther result) where TOther : INumberBase<TOther> => throw new NotImplementedException();
#endif

    public static Rational Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider) => throw new NotImplementedException();
    public static Rational Parse(string s, NumberStyles style, IFormatProvider? provider) => throw new NotImplementedException();
    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, [MaybeNullWhen(false)] out Rational result) => throw new NotImplementedException();
    public static bool TryParse([NotNullWhen(true)] string? s, NumberStyles style, IFormatProvider? provider, [MaybeNullWhen(false)] out Rational result) => throw new NotImplementedException();
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) => throw new NotImplementedException();
    public string ToString(string? format, IFormatProvider? formatProvider) => throw new NotImplementedException();
    public static Rational Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => throw new NotImplementedException();
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out Rational result) => throw new NotImplementedException();
    public static Rational Parse(string s, IFormatProvider? provider) => throw new NotImplementedException();
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out Rational result) => throw new NotImplementedException();
}
