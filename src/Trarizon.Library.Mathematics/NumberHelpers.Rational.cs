using System.Globalization;
using System.Numerics;

namespace Trarizon.Library.Mathematics;

internal static partial class NumberHelpers
{
#if NET7_0_OR_GREATER

    public static (T Numerator, T Denominator) TryParseRational<T>(ReadOnlySpan<char> s, NumberStyles style, NumberFormatInfo info, out bool success)
        where T : IBinaryInteger<T>
    {
        int index = 0;
        if (style.HasFlag(NumberStyles.AllowLeadingWhite))
        {
            while (index < s.Length)
            {
                var c = s[index];
                if (!IsWhite(c))
                    break;
                index++;
            }
        }

        T sign;
        if (style.HasFlag(NumberStyles.AllowLeadingSign))
        {
            sign = ParseLeadingSign(info, s[index..], out var readCount);
            index += readCount;
        }
        else
        {
            sign = T.One;
        }

        T number = T.Zero;
        ReadInteger(s, ref index, ref number);

        var sep = s[index];
        index++;
        T denominator;
        if (sep is '.')
        {
            denominator = T.One;
            ReadDenominator(s, ref index, ref number, ref denominator);
        }
        else if (sep is '/')
        {
            denominator = T.Zero;
            ReadInteger(s, ref index, ref denominator);
        }
        else
        {
            denominator = T.One;
        }

        var gcd = TraMath.Gcd(number, denominator);
        number = T.CopySign(sign, number / gcd);
        denominator /= gcd;

        success = true;
        return (number, denominator);

        static T ParseLeadingSign(NumberFormatInfo info, ReadOnlySpan<char> s, out int readCount)
        {
            var posSign = info.PositiveSign;
            if (posSign.Length <= s.Length)
            {
                if (s[..posSign.Length].SequenceEqual(posSign))
                {
                    readCount = posSign.Length;
                    return T.One;
                }
            }
            var negSign = info.NegativeSign;
            if (negSign.Length <= s.Length)
            {
                if (s[..negSign.Length].SequenceEqual(negSign))
                {
                    readCount = negSign.Length;
                    return -T.One;
                }
            }
            readCount = 0;
            return T.One;
        }

        static void ReadInteger(ReadOnlySpan<char> chars, ref int index, ref T num)
        {
            var ten = T.CreateChecked(10);
            while (index < chars.Length)
            {
                var ch = chars[index];
                var digit = GetDigit(ch);
                if (digit < 0)
                    break;
                index++;
                num = num * ten + T.CreateChecked(digit);
            }
        }

        static void ReadDenominator(ReadOnlySpan<char> chars, ref int index, ref T num, ref T den)
        {
            var ten = T.CreateChecked(10);
            while (index < chars.Length)
            {
                var ch = chars[index];
                var digit = GetDigit(ch);
                if (digit < 0)
                    break;
                index++;
                num = num * ten + T.CreateChecked(digit);
                den *= ten;

                var gcd = TraMath.Gcd(num, den);
                num /= gcd;
                den /= gcd;
            }
        }
    }

    private static bool IsWhite(char c) => c is ' ' or '\t' or '\n' or '\v' or '\f' or '\r';

    private static int GetDigit(char c)
    {
        if (c > '9')
            return -1;
        return c - '0';
    }

    public static bool TryFormatRational<T>(T numerator, T denominator, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        where T : IBinaryInteger<T>
    {
        if (T.IsZero(numerator) || denominator == T.One)
            return numerator.TryFormat(destination, out charsWritten, format, provider);

        if (!numerator.TryFormat(destination, out var w1, format, provider))
        {
            charsWritten = 0;
            return false;
        }
        if (destination.Length < w1 + 1)
        {
            charsWritten = 0;
            return false;
        }

        if (!numerator.TryFormat(destination, out var w2, format, provider))
        {
            charsWritten = 0;
            return false;
        }

        destination[w1] = '/';
        charsWritten = w1 + w2 + 1;
        return true;
    }

#endif
}
