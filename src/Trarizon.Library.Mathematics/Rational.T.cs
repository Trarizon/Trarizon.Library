using Microsoft.VisualBasic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using Trarizon.Library.Mathematics.Helpers;

namespace Trarizon.Library.Mathematics;

#if NET7_0_OR_GREATER

public readonly struct Rational<T> : IRational<Rational<T>, T>
    , INumber<Rational<T>>
    where T : IBinaryInteger<T>
{
    // No NaN, No Infinity
    // default(Rational<T>) as 0/0 means 0
    // always reduced to lowest terms, 0 always stored as 0/0, _denominator is always positive(except 0)
    private readonly T _numerator;
    private readonly T _denominator;

    public T Numerator => _numerator;
    public T Denominator => _denominator;

    public Rational(T numerator, T denominator) : this(numerator, denominator, true) { }

    private Rational(T numerator, T denominator, bool reduce)
    {
        if (reduce)
        {
            if (T.IsZero(denominator) && !T.IsZero(numerator))
                Throws.ThrowDivideByZero("Denominator cannot be 0");
            (numerator, denominator) = ReduceInternal(numerator, denominator);
        }
        (_numerator, _denominator) = (numerator, denominator);
    }

    #region Constants

    public static Rational<T> Zero => new(T.Zero, T.Zero, false);
    public static Rational<T> One => new(T.One, T.One, false);

    static int INumberBase<Rational<T>>.Radix => 2;
    static Rational<T> IAdditiveIdentity<Rational<T>, Rational<T>>.AdditiveIdentity => Zero;
    static Rational<T> IMultiplicativeIdentity<Rational<T>, Rational<T>>.MultiplicativeIdentity => One;

    #endregion

    #region Create

    #endregion

    #region Reduction CommonDenominator

    private static (T Numerator, T Denominator) ReduceInternal(T numerator, T denominator)
    {
        if (numerator == T.Zero)
            return (T.Zero, T.Zero);

        var gcd = TraMath.Gcd(numerator, denominator);
        return (numerator / gcd, denominator / gcd);
    }

    #endregion


    #region Operators

    private static Rational<T> CheckedAdd(Rational<T> l, Rational<T> r)
    {
        if (IsZero(l)) return r;
        if (IsZero(r)) return l;

        Debug.Assert(!IsZero(l._denominator) && !IsZero(r._denominator));
        checked
        {
            var (left, right) = TraMath.LcmMultipliers(l._denominator, r._denominator);
            var lnum = l._numerator * left;
            var rnum = r._numerator * right;
            return new Rational<T>(lnum + rnum, right);
        }
    }

    private static Rational<T> CheckedMultiply(Rational<T> l, Rational<T> r)
    {
        if (IsZero(l) || IsZero(r)) return Zero;

        var g1 = TraMath.Gcd(l._numerator, r._denominator);
        var g2 = TraMath.Gcd(l._denominator, r._numerator);
        var n1 = l._numerator / g1;
        var n2 = r._numerator / g2;
        var d1 = l._denominator / g2;
        var d2 = r._denominator / g1;
        checked
        {
            return new(n1 * n2, d1 * d2, false);
        }
    }

    private static Rational<T> CheckedModulus(Rational<T> l, Rational<T> r)
    {
        checked
        {
            // a/b % c/d 
            // a/b - [a/b / c/d] * c/d
            var quo = Floor(l / r);
            return l - quo * r;
        }
    }

    public static (T Quotient, Rational<T> Remainder) DivRem(Rational<T> l, Rational<T> r)
    {
        var quo = Floor(l / r);
        var rem = l - quo * r;
        return (quo, rem);
    }

    public static Rational<T> operator +(Rational<T> left, Rational<T> right) => CheckedAdd(left, right);
    public static Rational<T> operator -(Rational<T> left, Rational<T> right) => CheckedAdd(left, -right);
    public static Rational<T> operator *(Rational<T> left, Rational<T> right) => CheckedMultiply(left, right);
    public static Rational<T> operator /(Rational<T> left, Rational<T> right) => CheckedMultiply(left, Reciprocal(right));
    public static Rational<T> operator %(Rational<T> left, Rational<T> right) => CheckedModulus(left, right);

    public static Rational<T> operator ++(Rational<T> value) => value + One;
    public static Rational<T> operator --(Rational<T> value) => value - One;
    public static Rational<T> operator -(Rational<T> value) => new(-value._numerator, value._denominator, false);
    public static Rational<T> operator +(Rational<T> value) => value;

    #endregion

    #region Comparison

    private static int Compare(Rational<T> left, Rational<T> right)
    {
        if (left == right)
            return 0;
        if (GreaterThan(left, right))
            return 1;
        return -1;
    }

    private static bool GreaterThan(Rational<T> left, Rational<T> right)
    {
        var (ml, mr) = TraMath.LcmMultipliers(left._denominator, right._denominator);
        var lnum = left._numerator * ml;
        var rnum = right._numerator * mr;
        return lnum > rnum;
    }

    private static bool LessThan(Rational<T> left, Rational<T> right)
    {
        var (ml, mr) = TraMath.LcmMultipliers(left._denominator, right._denominator);
        var lnum = left._numerator * ml;
        var rnum = right._numerator * mr;
        return lnum < rnum;
    }

    public static bool operator >(Rational<T> left, Rational<T> right) => GreaterThan(left, right);
    public static bool operator >=(Rational<T> left, Rational<T> right) => !LessThan(left, right);
    public static bool operator <(Rational<T> left, Rational<T> right) => LessThan(left, right);
    public static bool operator <=(Rational<T> left, Rational<T> right) => !GreaterThan(left, right);
    public static bool operator ==(Rational<T> left, Rational<T> right) => left.Equals(right);
    public static bool operator !=(Rational<T> left, Rational<T> right) => !left.Equals(right);

    int IComparable.CompareTo(object? obj) => obj is Rational<T> r ? CompareTo(r) : Throws.ThrowInvalidOperation<int>();
    public int CompareTo(Rational<T> other) => Compare(this, other);
    public bool Equals(Rational<T> other) => _numerator == other._numerator && _denominator == other._denominator;
    public override bool Equals(object? obj) => obj is Rational<T> r && Equals(r);
    public override int GetHashCode() => HashCode.Combine(_numerator, _denominator);

    #endregion

    #region Conversion

    public static implicit operator Rational<T>(T value) => IsZero(value) ? Zero : new(value, T.One);
    public static explicit operator T(Rational<T> value) => value._numerator / value._denominator;

    #endregion

    #region Reciprocal Lerp Clamp Floor Ceiling Round Min Max

    public static Rational<T> Reciprocal(Rational<T> value)
    {
        if (IsZero(value))
            Throws.ThrowDivideByZero("Denominator cannot be 0");
        return new(value._denominator, value._numerator, false);
    }

    public static Rational<T> Lerp(Rational<T> min, Rational<T> max, Rational<T> amount)
        => min + amount * (max - min);

    public static Rational<T> Clamp(Rational<T> value, Rational<T> min, Rational<T> max)
    {
        if (min > max)
            Throws.ThrowArgument("min cannot be greater than max");
        if (value < min)
            return min;
        if (value > max)
            return max;
        return value;
    }

    public static T Floor(Rational<T> value)
    {
        if (IsZero(value))
            return T.Zero;
        if (IsPositive(value))
            return value._numerator / value._denominator;

        var (quo, rem) = T.DivRem(value._numerator, value._denominator);
        if (T.IsZero(rem))
            return quo;
        return quo - T.One;
    }

    public static T Ceiling(Rational<T> value)
    {
        if (IsZero(value))
            return T.Zero;
        if (IsNegative(value))
            return value._numerator / value._denominator;

        var (quo, rem) = T.DivRem(value._numerator, value._denominator);
        if (T.IsZero(rem))
            return quo;
        return quo + T.One;
    }

    public static Rational<T> Round(Rational<T> value)
    {
        if (IsZero(value))
            return value;

        // TODO: 四舍五入？银行家算法？
        throw new NotImplementedException();
    }

    public static Rational<T> Min(Rational<T> x, Rational<T> y)
        => x < y ? x : y;

    public static Rational<T> Max(Rational<T> x, Rational<T> y)
        => x > y ? x : y;

    public static Rational<T> Abs(Rational<T> value) => new(T.Abs(value._numerator), value._denominator);

    #endregion

    #region Interface

    static bool INumberBase<Rational<T>>.IsCanonical(Rational<T> value) => true;
    static bool INumberBase<Rational<T>>.IsComplexNumber(Rational<T> value) => false;
    public static bool IsEvenInteger(Rational<T> value)
    {
        var (quo, rem) = T.DivRem(value._numerator, value._denominator);
        return T.IsZero(rem) && T.IsEvenInteger(quo);
    }
    static bool INumberBase<Rational<T>>.IsFinite(Rational<T> value) => true;
    static bool INumberBase<Rational<T>>.IsImaginaryNumber(Rational<T> value) => false;
    static bool INumberBase<Rational<T>>.IsInfinity(Rational<T> value) => false;
    public static bool IsInteger(Rational<T> value) => IsZero(value) || value._denominator == T.One;
    static bool INumberBase<Rational<T>>.IsNaN(Rational<T> value) => false;
    public static bool IsNegative(Rational<T> value) => T.IsNegative(value._numerator);
    static bool INumberBase<Rational<T>>.IsNegativeInfinity(Rational<T> value) => false;
    static bool INumberBase<Rational<T>>.IsNormal(Rational<T> value) => !IsZero(value._numerator);
    public static bool IsOddInteger(Rational<T> value)
    {
        var (quo, rem) = T.DivRem(value._numerator, value._denominator);
        return T.IsZero(rem) && T.IsOddInteger(quo);
    }
    public static bool IsPositive(Rational<T> value) => T.IsPositive(value._numerator);
    static bool INumberBase<Rational<T>>.IsPositiveInfinity(Rational<T> value) => false;
    static bool INumberBase<Rational<T>>.IsRealNumber(Rational<T> value) => true;
    static bool INumberBase<Rational<T>>.IsSubnormal(Rational<T> value) => false;
    static bool INumberBase<Rational<T>>.IsZero(Rational<T> value) => T.IsZero(value._numerator);
    private static bool IsZero(Rational<T> value) => T.IsZero(value._numerator);
    public static Rational<T> MaxMagnitude(Rational<T> x, Rational<T> y) => Abs(x) > Abs(y) ? x : y;
    static Rational<T> INumberBase<Rational<T>>.MaxMagnitudeNumber(Rational<T> x, Rational<T> y) => Abs(x) > Abs(y) ? x : y;
    public static Rational<T> MinMagnitude(Rational<T> x, Rational<T> y) => Abs(x) < Abs(y) ? x : y;
    static Rational<T> INumberBase<Rational<T>>.MinMagnitudeNumber(Rational<T> x, Rational<T> y) => Abs(x) < Abs(y) ? x : y;

    #endregion

    #region Convert

    // TODO:

    static bool INumberBase<Rational<T>>.TryConvertFromChecked<TOther>(TOther value, [MaybeNullWhen(false)] out Rational<T> result)
    {
        if (typeof(TOther) == typeof(T))
        {
            result = (T)(object)value;
            return true;
        }

        if (TOther.IsInteger(value))
        {
            if (T.TryConvertFromChecked(value, out var checkedValue))
            {
                result = checkedValue;
                return true;
            }
        }
        result = default;
        return false;
    }
    static bool INumberBase<Rational<T>>.TryConvertFromSaturating<TOther>(TOther value, [MaybeNullWhen(false)] out Rational<T> result)
    {
        if (typeof(TOther) == typeof(T))
        {
            result = (T)(object)value;
            return true;
        }

        if (TOther.IsInteger(value))
        {
            if (T.TryConvertFromSaturating(value, out var saturatingValue))
            {
                result = saturatingValue;
                return true;
            }
        }
        result = default;
        return false;
    }
    static bool INumberBase<Rational<T>>.TryConvertFromTruncating<TOther>(TOther value, [MaybeNullWhen(false)] out Rational<T> result)
    {
        result = default;
        return false;
    }
    static bool INumberBase<Rational<T>>.TryConvertToChecked<TOther>(Rational<T> value, [MaybeNullWhen(false)] out TOther result)
    {
        if (value._denominator == T.One || T.IsZero(value._numerator))
        {
            return T.TryConvertToChecked(value._numerator, out result);
        }

        if (typeof(TOther) == typeof(T))
        {
            result = (TOther)(object)(T)value;
            return true;
        }
        if (NumberHelpers.IsIBinaryInteger<TOther>())
        {
            var val = value._numerator / value._denominator;
            return T.TryConvertToChecked(val, out result);
        }
        if (NumberHelpers.IsIFloatingPoint<TOther>())
        {
            TOther? fn, fd;
            if (!TOther.TryConvertFromChecked(value._numerator, out fn) && !T.TryConvertToChecked(value._numerator, out fn))
            {
                result = default;
                return false;
            }
            if (!TOther.TryConvertFromChecked(value._denominator, out fd) && !T.TryConvertToChecked(value._denominator, out fd))
            {
                result = default;
                return false;
            }
            result = fn / fd;
            return true;
        }
        result = default;
        return false;
    }
    static bool INumberBase<Rational<T>>.TryConvertToSaturating<TOther>(Rational<T> value, [MaybeNullWhen(false)] out TOther result)
        => TryConvertToSaturating(value, out result);
    static bool INumberBase<Rational<T>>.TryConvertToTruncating<TOther>(Rational<T> value, [MaybeNullWhen(false)] out TOther result)
        => TryConvertToSaturating(value, out result);

    private static bool TryConvertToSaturating<TOther>(Rational<T> value, [MaybeNullWhen(false)] out TOther result) where TOther : INumberBase<TOther>
    {
        if (value._denominator == T.One || T.IsZero(value._numerator))
        {
            return T.TryConvertToSaturating(value._numerator, out result);
        }
        if (typeof(TOther) == typeof(T))
        {
            result = (TOther)(object)(T)value;
            return true;
        }
        if (NumberHelpers.IsIBinaryInteger<TOther>())
        {
            var val = value._numerator / value._denominator;
            return T.TryConvertToSaturating(val, out result);
        }
        if (NumberHelpers.IsIFloatingPoint<TOther>())
        {
            var (quo, rem) = T.DivRem(value._numerator, value._denominator);
            TOther? fq, fr;
            if (!TOther.TryConvertFromSaturating(quo, out fq) && !T.TryConvertToSaturating<TOther>(quo, out fq))
            {
                result = default;
                return false;
            }
            if (!TOther.TryConvertFromSaturating(rem, out fr) && !T.TryConvertToSaturating<TOther>(rem, out fr))
            {
                result = default;
                return false;
            }
            result = fq + fr / fq;
            return true;
        }
        result = default;
        return false;
    }

    #endregion

    #region Parsing Formatting


    public static Rational<T> Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider)
    {
        if (!TryParse(s, provider, out var result))
            Throws.ThrowFormatException($"Invalid input string '{s}'");
        return result;
    }
    public static Rational<T> Parse(string s, NumberStyles style, IFormatProvider? provider) => Parse(s.AsSpan(), style, provider);
    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, [MaybeNullWhen(false)] out Rational<T> result)
    {
        var (num, den) = NumberHelpers.TryParseRational<T>(s, style, NumberFormatInfo.GetInstance(provider), out var success);
        result = new Rational<T>(num, den, false);
        return success;
    }
    public static bool TryParse([NotNullWhen(true)] string? s, NumberStyles style, IFormatProvider? provider, [MaybeNullWhen(false)] out Rational<T> result) => TryParse(s.AsSpan(), provider, out result);
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        => NumberHelpers.TryFormatRational(_numerator, _denominator, destination, out charsWritten, format, provider);
    public string ToString(string? format, IFormatProvider? formatProvider) => throw new NotImplementedException();
    public static Rational<T> Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        if (!TryParse(s, provider, out var result))
            Throws.ThrowFormatException($"Invalid input string '{s}'");
        return result;
    }
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out Rational<T> result) => TryParse(s, NumberStyles.Integer, provider, out result);
    public static Rational<T> Parse(string s, IFormatProvider? provider) => Parse(s.AsSpan(), provider);
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out Rational<T> result) => TryParse(s.AsSpan(), provider, out result);
    public override string ToString() => ToString(null, null);

    #endregion
}

#endif
