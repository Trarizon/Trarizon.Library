using CommunityToolkit.Diagnostics;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using Int = int;

#if NETSTANDARD
using BitOperations = Trarizon.Library.Mathematics.Helpers.PfBitOperations;
#endif

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

    #region Constants

    public static Rational One => new Rational(1, 1);

    public static Rational NegativeOne => new Rational(-1, 1);

    public static Rational Zero => new Rational(0, 1);

    public static Rational PositiveInfinity => new Rational(1, 0);

    public static Rational NegativeInfinity => new Rational(-1, 0);

    public static Rational NaN => new Rational(0, 0);

    public static Rational MaxValue => new Rational(Int.MaxValue, 1);

    public static Rational MinValue => new Rational(Int.MinValue, 1);

#if NET7_0_OR_GREATER

    static int INumberBase<Rational>.Radix => 2;

    static Rational IAdditiveIdentity<Rational, Rational>.AdditiveIdentity => Zero;

    static Rational IMultiplicativeIdentity<Rational, Rational>.MultiplicativeIdentity => One;

#endif

    #endregion

    #region Operator implementations

    #region Reduction CommonDenominator

    private static (long Numerator, long Denominator) Reduce(long numerator, long denominator)
    {
        switch (numerator, denominator) {
            case (0, 0): return (0, 0);
            case ( > 0, 0): return (1, 0);
            case ( < 0, 0): return (-1, 0);
            case (0, _): return (0, 1);
            default: {
                var gcd = TraMath.GreatestCommonDivisor(numerator, denominator);
                return (numerator / gcd, denominator / gcd);
            }
        }
    }

    private static (int numerator, int Denominator) Reduce(int numerator, int denominator)
    {
        switch (numerator, denominator) {
            case (0, 0): return (0, 0);
            case ( > 0, 0): return (1, 0);
            case ( < 0, 0): return (-1, 0);
            case (0, _): return (0, 1);
            default: {
                var gcd = TraMath.GreatestCommonDivisor(numerator, denominator);
                return (numerator / gcd, denominator / gcd);
            }
        }
    }

    public static Rational Reduce(Rational number)
    {
        var (num, den) = Reduce(number._numerator, number._denominator);
        return new(num, den);
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

    private static long DangerousCommonDenominatorUnchecked(Rational left, Rational right, out long numeratorLeft, out long numeratorRight)
    {
        if (left._denominator == right._denominator) {
            numeratorLeft = left._numerator;
            numeratorRight = right._numerator;
            return numeratorLeft;
        }
        var ldeno = (long)left._denominator;
        var rdeno = (long)right._denominator;
        numeratorLeft = left._numerator * rdeno;
        numeratorRight = right._numerator * ldeno;
        return ldeno * rdeno;
    }

    #endregion

    public static Rational Reciprocal(Rational num)
        => new Rational(num._denominator, num._numerator);

    public static Rational Lerp(Rational min, Rational max, Rational amount)
    {
        return min + amount * (max - min);
    }

    public static Rational Clamp(Rational value, Rational min, Rational max)
    {
        Guard.IsLessThanOrEqualTo(min, max);
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }

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

    public static Rational Round(Rational num)
    {
        if (!IsFinite(num))
            return num;

        if (num._numerator == 0)
            return 0;

        var div = Math.DivRem(num._numerator, num._denominator, out var rem);

        if (div > 0) {
            if (num._denominator / rem > 2)
                return div;
            else
                return div + 1;
        }
        else {
            if (num._denominator / rem < -2)
                return div;
            else
                return div - 1;
        }
    }

    public static Rational Truncate(Rational num)
    {
        if (!IsFinite(num))
            return num;

        if (num._numerator == 0)
            return 0;

        var div = Math.DivRem(num._numerator, num._denominator, out var rem);

        if (div > 0) {
            if (num._denominator / rem >= 2)
                return div;
            else
                return div + 1;
        }
        else {
            if (num._denominator / rem <= -2)
                return div;
            else
                return div - 1;
        }
    }

    public static Rational Max(Rational x, Rational y)
    {
        if (x != y) {
            if (IsNaN(x))
                return y < x ? x : y;
            return x;
        }
        return IsNegative(y) ? x : y;
    }

    public static Rational Min(Rational x, Rational y)
    {
        if (x != y) {
            if (!IsNaN(x)) {
                return x < y ? x : y;
            }
            return x;
        }

        return IsNegative(x) ? x : y;
    }

    #region Basic math operators

    private static Rational Add(Rational l, Rational r)
    {
        switch (l._numerator, l._denominator, r._numerator, r._denominator) {
            case (_, not 0, _, not 0): {
                var den = DangerousCommonDenominatorUnchecked(l, r, out var lnum, out var rnum);
                var num = lnum + rnum;
                return FromLongUnchecked(num, den);
            }
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

    private static Rational CheckedAdd(Rational l, Rational r)
    {
        switch (l._numerator, l._denominator, r._numerator, r._denominator) {
            case (_, not 0, _, not 0): {
                var den = DangerousCommonDenominatorUnchecked(l, r, out var lnum, out var rnum);
                var num = lnum + rnum;
                return FromLongChecked(num, den);
            }
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
        var den = (long)l._denominator * (long)r._denominator;
        var num = (long)l._numerator * (long)r._numerator;
        return FromLongUnchecked(num, den);
    }

    private static Rational CheckedMultiple(Rational l, Rational r)
    {
        var den = (long)l._denominator * (long)r._denominator;
        var num = (long)l._numerator * (long)r._numerator;
        return FromLongChecked(num, den);
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

        var den = DangerousCommonDenominatorUnchecked(left, right, out var numl, out var numr);
        var mod = numl % numr;
        return FromLongUnchecked(mod, den);
    }

    #endregion

    #region Comparison

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

    public bool Equals(Rational other) => Equals(this, other);

    public override bool Equals(object? obj) => obj is Rational number && Equals(number);

    public override int GetHashCode()
    {
        var num = Reduce(this);
        return HashCode.Combine(num._numerator, num._denominator);
    }
    public int CompareTo(Rational other) => Compare(this, other);

    int IComparable.CompareTo(object? obj)
    {
        if (obj is not Rational number)
            return ThrowHelper.ThrowInvalidOperationException<int>();
        return Compare(this, number);
    }

    #endregion

    #endregion

    #region Convert

    public static implicit operator Rational(byte n) => new Rational(n, 1);
    public static implicit operator Rational(sbyte n) => new Rational(n, 1);
    public static implicit operator Rational(short n) => new Rational(n, 1);
    public static implicit operator Rational(ushort n) => new Rational(n, 1);
    public static implicit operator Rational(int n) => new Rational(n, 1);
    public static explicit operator Rational(uint value) => new((Int)value, 1);
    public static explicit operator Rational(long value) => new((Int)value, 1);
    public static explicit operator Rational(float value) => FromSingle(value);
    public static explicit operator Rational(double value) => FromDouble(value);

    public static explicit operator checked Rational(uint value) => new(checked((Int)value), 1);
    public static explicit operator checked Rational(long value) => new(checked((Int)value), 1);

    public static explicit operator int(Rational value) => value._numerator / value._denominator;
    public static implicit operator float(Rational value) => (float)value._numerator / value._denominator;
    public static implicit operator double(Rational value) => (double)value._numerator / value._denominator;
    public static implicit operator decimal(Rational value) => (decimal)value._numerator / value._denominator;

    private static Rational FromSingle(float value)
    {
        //const int IntMaxShift = 31; // sizeof(Int) * 8 - 1;
        const int ExpShift = 23;
        const int ExpMask = 0xFF;
        const int ManMask = 0x7FFFFF;
        const int DenShiftDelta = 127;

        int bits = Unsafe.As<float, int>(ref value);
        int sign = bits >= 0 ? 1 : -1;
        int exponent = (bits >> ExpShift) & ExpMask;
        int mantissa = bits & ManMask;

        if (exponent == 0)
            return Zero;

        // Inf
        if (exponent == ExpMask) {
            if (mantissa == 0)
                return new(sign, 0);
            else
                return NaN;
        }

        mantissa |= 1 << ExpShift;
        int numerator = mantissa;
        int shift = ExpShift + DenShiftDelta - exponent;

        // return (sign * numerator) / (1 << shift);

        //                     (1 << 23) <= numerator < (1 << 24)
        // int.MaxValue + 1 == (1 << 31) <= numerator << (31 - 23) < (1 << 32)

        // shift < 0, denominator < 1
        // If shift denominator to 1, new_numerator is (numerator << shift),
        // if new_numerator > int.Max/Min, return inf
        if (shift < 23 - 31) {
            // numerator = numerator << 31 - 23  >= int.MaxValue + 1
            // denominator = 1
            if (sign >= 0)
                return PositiveInfinity;
            else {
                // numerator == 1 << 23
                // after: numerator == 1 << 23 << 31 >> 23 = 1 << 31
                if (numerator == 1 << ExpShift)
                    return MinValue;
                return NegativeInfinity;
            }
        }
        if (shift < 0) {
            numerator <<= -shift;
            return sign * numerator;
        }

        // shift >= 0, denominator >= 1
        else {
            var trailing = BitOperations.TrailingZeroCount(numerator);
            Debug.Assert(trailing <= 23);

            // 110000 / 100 => 1100 / 1
            if (shift <= trailing) {
                return new Rational(numerator >> shift, 1);
            }
            // 1100 / 10000 => 11 / 100
            // denominator may > int.MaxValue, in other words, (shift >= 31)
            else {
                // reduced_numerator = numerator >> trailing;
                int reducedShift = shift - trailing;

                // If after reduce, shift is still >= 31, denominator is still > int.MaxValue,
                // We truncate numerator and get an inaccurate result;
                // NOTE: We make denominator always positive here, if denominator is negative,
                //       the condition should be shift > 31 as -int.MinValue is (int.MaxValue - 1)
                if (reducedShift >= 31) {
                    // max_numerator_shift is 24(numerator will be 0)
                    var numeratorShift = shift - 31;

                    // numerator will be 0
                    if (numeratorShift > 23) {
                        return Zero;
                    }
                    else {
                        // denonimator:
                        // (1 << shift) >> numeratorShift
                        // (1 << shift) >> shift << 31
                        // 1 << 31 (appro int.MaxValue)
                        return new Rational(numerator >> numeratorShift, Int.MaxValue);
                    }
                }

                // After reduce, shift is < 31, denominator is <= int.MaxValue,
                // The result value is ok
                else {
                    // numerator:            denominator:
                    // numerator             1 << shift
                    // numerator >> trailing 1 << shift >> trailing
                    // numerator >> trailing 1 << (shift - trailing)
                    return new Rational(numerator >> trailing, 1 << reducedShift);
                }
            }
        }
    }

    private static Rational FromDouble(double value)
    {
        return FromSingle((float)value);
        /*

        const int IntMaxShift = sizeof(Int) * 8 - 2;
        const int ExpShift = 52;
        const long ExpMask = 0x7FF;
        const long ManMask = 0x0FFFFFFFFFFFFF;
        const int DenShift = 1023;

        long bits = Unsafe.As<double, long>(ref value);
        int sign = bits >= 0 ? 1 : -1;
        int exponent = (int)((bits >> ExpShift) & ExpMask);
        long mantissa = bits & ManMask;

        if (exponent == 0) {
            return Zero;
        }

        // Inf
        if (exponent == ExpMask) {
            if (mantissa == 0)
                return new(sign, 0);
            else
                return NaN;
        }

        mantissa |= 1 << ExpShift;
        long numerator = mantissa;
        int shift = ExpShift + DenShift - exponent;

        // When shift < 0, the number is large, and denominator is 1
        // We left shift numerator to calc the result

        // Large than max, resulting inf

        if (shift < 0) {
            // numerator is always > int.Max, so its always too large if shift < 0
            return new Rational(sign, 0);
        }

        // Try reduce, we know denominator is always a pow2 number
        else {
            var trailing = BitOperations.TrailingZeroCount(numerator);

            // numerator / 1 << shift
            // (numerator / trailing) / (1 << shift - trailing)


            numerator >>= trailing;
            if (sign >= 0) {
                if(numerator > Int.MaxValue)
            }

            // ------

            // numerator is greater than int.MaxValue even after reduce
            if (trailing < ExpShift - IntMaxShift) {
                var minShift = ExpShift - IntMaxShift - trailing;
                if (shift < minShift)
                    return new Rational(sign, 0);
                numerator >>= minShift;
            }

            // 110000 / 100 => 1100 / 1
            if (shift <= trailing) {
                numerator >>= shift;
                if (sign > 0) {
                    if (numerator > int.MaxValue)
                        // overflow
                        return MaxValue;
                }
                else {
                    if (-numerator < int.MinValue)
                        // overflow
                        return MinValue;
                }
                return (int)numerator;
            }
            // 11100 / 10000 => 111 / 100
            else {
                int reducedShift = shift - trailing;

                // If after reduce, denominator is still greater than int.MaxValue,
                // we truncate numerator and get an inaccurate result
                if (reducedShift > IntMaxShift) {
                    int numeratorShift = shift - IntMaxShift;
                    if (numeratorShift > ExpShift)
                        return Zero;

                    // reduced numerator is greater than int.MaxValue
                    if (numeratorShift < ExpShift - IntMaxShift) {
                        int shiftMore = ExpShift - IntMaxShift - numeratorShift;
                        // numerator:                               denominator:
                        // numerator >> numeratorShift              1 << IntMaxShift
                        // numerator >> numeratorShift >> shiftMore 1 << IntMaxShift >> shiftMore
                        return new Rational(Int.MaxValue, 1 << IntMaxShift >> shiftMore);
                    }
                    else {
                        numerator >>= numeratorShift;
                        Debug.Assert(numerator <= Int.MaxValue);
                        return new Rational((Int)numerator, Int.MaxValue);
                    }
                }
                // After reduce, denominator is less than int.MaxValue
                else {

                }
            }
        }

        */
    }

    private static Rational FromLongUnchecked(long numerator, long denominator)
    {
        (numerator, denominator) = Reduce(numerator, denominator);
        return new((int)numerator, (int)denominator);
    }

    private static Rational FromLongChecked(long numerator, long denominator)
    {
        (numerator, denominator) = Reduce(numerator, denominator);
        checked { return new((int)numerator, (int)denominator); }
    }

    #endregion

    #region Operators

    public static Rational operator +(Rational left, Rational right) => Add(left, right);
    public static Rational operator -(Rational left, Rational right) => Add(left, -right);
    public static Rational operator *(Rational left, Rational right) => Multiple(left, right);
    public static Rational operator /(Rational left, Rational right) => Multiple(left, Reciprocal(right));
    public static Rational operator %(Rational left, Rational right) => Modulus(left, right);

    public static Rational operator checked +(Rational left, Rational right) => CheckedAdd(left, right);
    public static Rational operator checked -(Rational left, Rational right) => CheckedAdd(left, -right);
    public static Rational operator checked *(Rational left, Rational right) => CheckedMultiple(left, right);
    public static Rational operator checked /(Rational left, Rational right) => CheckedMultiple(left, Reciprocal(right));

    public static Rational operator --(Rational value) => new Rational(value._numerator - value._denominator, value._denominator);
    public static Rational operator ++(Rational value) => new Rational(value._numerator + value._denominator, value._denominator);
    public static Rational operator -(Rational value) => new Rational(-value._numerator, value._denominator);
    public static Rational operator +(Rational value) => value;

    public static Rational operator checked --(Rational value) => new Rational(checked(value._numerator - value._denominator), value._denominator);
    public static Rational operator checked ++(Rational value) => new Rational(checked(value._numerator + value._denominator), value._denominator);
    public static Rational operator checked -(Rational value) => new Rational(checked(-value._numerator), value._denominator);

    public static bool operator ==(Rational left, Rational right) => left.Equals(right);
    public static bool operator !=(Rational left, Rational right) => !left.Equals(right);
    public static bool operator >(Rational left, Rational right) => IsGreater(left, right);
    public static bool operator >=(Rational left, Rational right) => !IsGreater(right, left);
    public static bool operator <(Rational left, Rational right) => IsGreater(right, left);
    public static bool operator <=(Rational left, Rational right) => !IsGreater(left, right);

    #endregion

    #region INumber

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
        return value._denominator is not 0;
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

    #endregion

#if NET7_0_OR_GREATER

    public static bool TryConvertFromChecked<TOther>(TOther value, [MaybeNullWhen(false)] out Rational result) where TOther : INumberBase<TOther> => throw new NotImplementedException();
    public static bool TryConvertFromSaturating<TOther>(TOther value, [MaybeNullWhen(false)] out Rational result) where TOther : INumberBase<TOther> => throw new NotImplementedException();
    public static bool TryConvertFromTruncating<TOther>(TOther value, [MaybeNullWhen(false)] out Rational result) where TOther : INumberBase<TOther> => throw new NotImplementedException();
    public static bool TryConvertToChecked<TOther>(Rational value, [MaybeNullWhen(false)] out TOther result) where TOther : INumberBase<TOther> => throw new NotImplementedException();
    public static bool TryConvertToSaturating<TOther>(Rational value, [MaybeNullWhen(false)] out TOther result) where TOther : INumberBase<TOther> => throw new NotImplementedException();
    public static bool TryConvertToTruncating<TOther>(Rational value, [MaybeNullWhen(false)] out TOther result) where TOther : INumberBase<TOther> => throw new NotImplementedException();

#endif

    #region Parse Format ToString

    public static Rational Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider)
    {
        if (!TryParse(s, style, provider, out var result))
            ThrowHelper.ThrowInvalidOperationException("Invalid ration number format");
        return result;
    }

    public static Rational Parse(string s, NumberStyles style, IFormatProvider? provider) => Parse(s.AsSpan(), style, provider);
    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, [MaybeNullWhen(false)] out Rational result) => TryParseInternal(s, style, NumberFormatInfo.GetInstance(provider), out result);
    public static bool TryParse([NotNullWhen(true)] string? s, NumberStyles style, IFormatProvider? provider, [MaybeNullWhen(false)] out Rational result) => TryParse(s.AsSpan(), style, provider, out result);
    public static Rational Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => Parse(s, NumberStyles.Number, provider);
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out Rational result) => TryParse(s, NumberStyles.Number, provider, out result);
    public static Rational Parse(string s, IFormatProvider? provider) => Parse(s, NumberStyles.Number, provider);
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out Rational result) => TryParse(s, NumberStyles.Number, provider, out result);

    private static bool TryParseInternal(ReadOnlySpan<char> s, NumberStyles style, NumberFormatInfo info, out Rational result)
    {
        int index = 0;

        if (style.HasFlag(NumberStyles.AllowLeadingWhite)) {
            while (index < s.Length) {
                var ch = s[index];
                if (!IsWhite(ch)) {
                    break;
                }
                index++;
                continue;
            }
        }

        int leadingSign;

        if (style.HasFlag(NumberStyles.AllowLeadingSign)) {
            leadingSign = ParseLeadingSign(s.Slice(index), out var readCount);
            index += readCount;
        }
        else {
            leadingSign = 1;
        }

        long number = 0;

        ReadInteger(s, ref index, ref number);

        var sepch = s[index];
        index++;
        long denominator;
        if (sepch is '.') {
            denominator = 1;
            ReadDenominator(s, ref index, ref number, ref denominator);
        }
        else if (sepch is '/') {
            denominator = 0;
            ReadInteger(s, ref index, ref denominator);
        }
        else {
            denominator = 1;
        }

        var gcd = TraMath.GreatestCommonDivisor(number, denominator);
        number = number / gcd * leadingSign;
        denominator /= gcd;
        if (!(number is >= int.MinValue and <= int.MaxValue && denominator is >= int.MinValue and < int.MaxValue)) {
            result = default;
            return false;
        }

        if (style.HasFlag(NumberStyles.AllowTrailingWhite)) {
            while (index < s.Length) {
                var ch = s[index];
                if (!IsWhite(ch)) {
                    result = default;
                    return false;
                }
                index++;
                continue;
            }
        }

        result = new Rational((int)number, (int)denominator);
        return true;

        int ParseLeadingSign(ReadOnlySpan<char> s, out int readCount)
        {
            var posSign = info.PositiveSign;
            if (posSign.Length <= s.Length) {
                if (s.Slice(0, posSign.Length).SequenceEqual(posSign)) {
                    readCount = posSign.Length;
                    return 1;
                }
            }
            var negSign = info.NegativeSign;
            if (negSign.Length <= s.Length) {
                if (s.Slice(0, negSign.Length).SequenceEqual(negSign)) {
                    readCount = negSign.Length;
                    return -1;
                }
            }
            readCount = 0;
            return 1;
        }

        static void ReadInteger(ReadOnlySpan<char> s, ref int index, ref long num)
        {
            while (index < s.Length) {
                var ch = s[index];
                var digit = GetDigit(ch);
                if (digit < 0)
                    break;
                index++;
                num = num * 10 + digit;
            }
        }

        static void ReadDenominator(ReadOnlySpan<char> s, ref int index, ref long num, ref long den)
        {
            while (index < s.Length) {
                var ch = s[index];
                var digit = GetDigit(ch);
                if (digit < 0)
                    break;
                index++;
                num = num * 10 + digit;
                den *= 10;
            }
        }
    }

    private static bool IsWhite(char ch)
        => ch is '\t' or '\n' or '\v' or '\f' or '\r' or ' ';

    private static int GetDigit(char ch)
    {
        if (ch >= '9')
            return -1;
        else
            return ch - '0';
    }

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        if (_denominator == 0) {
            var str = _numerator switch
            {
                0 => "NaN",
                > 0 => "+∞",
                _ => (ReadOnlySpan<char>)"-∞",
            };
            return TryWrite(destination, str, out charsWritten);
        }
        else if (_denominator is 1) {
#if NET7_0_OR_GREATER
            return _numerator.TryFormat(destination, out charsWritten, format, provider);
#else
            return TryWrite(destination, _numerator.ToString(format.ToString(), provider), out charsWritten);
#endif
        }
        else {
#if NET7_0_OR_GREATER
            if (_numerator.TryFormat(destination, out var w, format, provider)) {
#else
            var formatstr = format.ToString();
            if (TryWrite(destination, _numerator.ToString(formatstr, provider), out var w)) {
#endif
                if (destination.Length < w + 1) {
                    charsWritten = w;
                    return false;
                }

#if NET7_0_OR_GREATER
                if (_denominator.TryFormat(destination[(w + 1)..], out var w2, format, provider)) {
#else
                if (TryWrite(destination.Slice(w + 1), _denominator.ToString(formatstr, provider), out var w2)) {
#endif
                    destination[w] = '/';
                    charsWritten = w + w2 + 1;
                    return true;
                }
            }
            if (destination.Length < w + 1) {
                charsWritten = w;
                return false;
            }
        }
        charsWritten = default;
        return false;

        static bool TryWrite(Span<char> destination, ReadOnlySpan<char> text, out int charsWritten)
        {
            if (destination.Length > text.Length) {
                text.CopyTo(destination);
                charsWritten = text.Length;
                return true;
            }
            charsWritten = default;
            return false;
        }
    }

    public string ToString(string? format, IFormatProvider? formatProvider)
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
            return _numerator.ToString(format, formatProvider);
        }
        else {
            return $"{_numerator.ToString(format, formatProvider)}/{_denominator.ToString(format, formatProvider)}";
        }
    }

    public override string ToString() => ToString(null, null);

    #endregion
}
