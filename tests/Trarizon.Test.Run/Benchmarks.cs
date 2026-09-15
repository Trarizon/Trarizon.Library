using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Trarizon.Library.Collections;
using Trarizon.Library.Text;
using Trarizon.Library.Wrappers;

using NumberText2 = Trarizon.Library.Text.NumberText;

#pragma warning disable TRALIB

namespace Trarizon.Test.Run;

[MemoryDiagnoser]
public class Benchmarks
{
    private const int TestInt = int.MaxValue;
    private const long TestLong = long.MaxValue;
    private const uint TestUInt = 222222222;
    private const ulong TestULong = 222222222222;

    [Benchmark]
    public string NumberText_UInt() => NumberText.NumberToChinese(TestUInt);

    [Benchmark]
    public string NumberText2_UInt() => NumberText2.NumberToChinese(TestUInt);

    [Benchmark]
    public string NumberText_ULong() => NumberText.NumberToChinese(TestULong);

    [Benchmark]
    public string NumberText2_ULong() => NumberText2.NumberToChinese(TestULong);

}
public static partial class NumberText
{
    public static string NumberToChinese(int number, ToChineseOptions options = ToChineseOptions.None)
    {
        // 负二十一亿四千七百四十八万三千六百四十八
        var sb = (stackalloc char[20]);
        NumberToChinese(number, options, sb, out var length);
        return sb[..length].ToString();
    }

    public static string NumberToChinese(uint number, ToChineseOptions options = ToChineseOptions.None)
    {
        var sb = (stackalloc char[19]);
        NumberToChinese(number, options, sb, out var length);
        return sb[..length].ToString();
    }

    public static string NumberToChinese(long number, ToChineseOptions options = ToChineseOptions.None)
    {
        var sb = (stackalloc char[40]);
        NumberToChinese(number, options, sb, out var length);
        return sb[..length].ToString();
    }

    public static string NumberToChinese(ulong number, ToChineseOptions options = ToChineseOptions.None)
    {
        var sb = (stackalloc char[41]);
        NumberToChinese(number, options, sb, out var length);
        return sb[..length].ToString();
    }

    private static void GetChineseNumberUnits(ToChineseOptions options, out char negative, out ReadOnlySpan<char> digits, out ReadOnlySpan<char> tens, out ReadOnlySpan<char> wans)
    {
        var financial = (options & ToChineseOptions.FinancialNumerals) != 0;
        var hant = (options & ToChineseOptions.TraditionalChinese) != 0;
        switch (financial, hant)
        {
            case (false, false):
                negative = '负';
                digits = ['零', '一', '二', '三', '四', '五', '六', '七', '八', '九', '两'];
                tens = ['十', '百', '千'];
                wans = ['万', '亿', '兆', '京', '垓', '秭', '穰', '沟', '涧', '正', '载', '极']; // 恒河沙 阿僧祗 那由他 不可思议 无量大数
                break;
            case (false, true):
                negative = '負';
                digits = ['零', '一', '二', '三', '四', '五', '六', '七', '八', '九', '兩'];
                tens = ['十', '百', '千'];
                wans = ['萬', '億', '兆', '京', '垓', '秭', '穰', '溝', '澗', '正', '載', '極']; // 恆河沙 阿僧祇 那由他 不可思議 無量大數
                break;
            case (true, false):
                negative = '负';
                digits = ['零', '壹', '贰', '叁', '肆', '伍', '陆', '柒', '捌', '玖', '两'];
                tens = ['拾', '佰', '仟'];
                wans = ['万', '亿', '兆', '京', '垓', '秭', '穰', '沟', '涧', '正', '载', '极'];
                break;
            case (true, true):
                negative = '負';
                digits = ['零', '壹', '貳', '參', '肆', '伍', '陸', '柒', '捌', '玖', '兩'];
                tens = ['拾', '佰', '仟'];
                wans = ['萬', '億', '兆', '京', '垓', '秭', '穰', '溝', '澗', '正', '載', '極'];
                break;
        }
    }

    public static void NumberToChinese(int number, ToChineseOptions options, Span<char> span, out int length)
    {
        GetChineseNumberUnits(options, out var negative, out var digits, out var tens, out var wans);
        wans = wans[..2];
        if (number < 0)
        {
            span[0] = negative;
            NumberToChineseCoreUInt(unchecked((uint)-number), digits, tens, wans, span[1..], out length, options);
            length++;
            return;
        }
        NumberToChineseCoreUInt(unchecked((uint)number), digits, tens, wans, span, out length, options);
    }

    public static void NumberToChinese(uint number, ToChineseOptions options, Span<char> span, out int length)
    {
        GetChineseNumberUnits(options, out var negative, out var digits, out var tens, out var wans);
        wans = wans[..2];
        NumberToChineseCoreUInt(number, digits, tens, wans, span, out length, options);
    }

    public static void NumberToChinese(long number, ToChineseOptions options, Span<char> span, out int length)
    {
        GetChineseNumberUnits(options, out var negative, out var digits, out var tens, out var wans);
        wans = wans[..4];
        if (number < 0)
        {
            span[0] = negative;
            NumberToChineseCoreULong(unchecked((ulong)-number), digits, tens, wans, span[1..], out length, options);
            length++;
            return;
        }
        NumberToChineseCoreULong(unchecked((ulong)number), digits, tens, wans, span, out length, options);
    }

    public static void NumberToChinese(ulong number, ToChineseOptions options, Span<char> span, out int length)
    {
        GetChineseNumberUnits(options, out var negative, out var digits, out var tens, out var wans);
        wans = wans[..4];
        NumberToChineseCoreULong(number, digits, tens, wans, span, out length, options);
    }

    private static ReadOnlySpan<uint> Pow10_UInt => [10, 100, 1000];
    private static ReadOnlySpan<uint> Pow10000_UInt => [1_0000, 1_0000_0000];
    private static ReadOnlySpan<ulong> Pow10000_ULong => [1_0000, 1_0000_0000, 1_0000_0000_0000, 1_0000_0000_0000_0000];

    private static void NumberToChineseCoreUInt(uint number, ReadOnlySpan<char> digits, ReadOnlySpan<char> tens, ReadOnlySpan<char> wans, Span<char> span, out int length, ToChineseOptions options)
    {
        if (number is 0)
        {
            span[0] = digits[0];
            length = 1;
            return;
        }

        var isb = 0;

        var useYiShiForTen = (options & ToChineseOptions.YiShiForTen) != 0;
        var useLiangForSingle = (options & ToChineseOptions.UseLiangForTwoWanUnits) != 0;
        var useLiangForHundred = (options & ToChineseOptions.UseLiangForTwoHundred) != 0;
        var useLiangForThousand = (options & ToChineseOptions.UseLiangForTwoThousand) != 0;

        int requiresZero = 0; // 0 no_output_now, 1 not_required, 2 required 
        for (int i = wans.Length - 1; i >= 0; i--)
        {
            var unit = Pow10000_UInt[i];
#if NET9_0_OR_GREATER
            var (digit, rem) = Math.DivRem(number, unit);
#else
            uint digit, rem;
            if (number <= (uint)int.MaxValue)
            {
                digit = unchecked((uint)Math.DivRem(unchecked((int)number), unchecked((int)unit), out int remI));
                rem = unchecked((uint)remI);
            }
            else
            {
                digit = number / unit;
                rem = number % unit;
            }
#endif

            if (digit > 0)
            {
                if (requiresZero is 2)
                {
                    span[isb++] = digits[0];
                }
                isb += NumberToChineseCoreUnder10000(digit, digits, tens, requiresZero, span[isb..], useYiShiForTen, useLiangForSingle, useLiangForHundred, useLiangForThousand);
                span[isb++] = wans[i];
                requiresZero = 1;

                number = rem;
            }
            else
            {
                if (requiresZero == 1)
                {
                    // requiresZero == 1 : 一整个万块都是0，此处应当补零，且后续的万内数字不需要前置零
                    // requiresZero == 0 && ipz == 1 : 需要外部要求需要前导0
                    requiresZero = 2;
                }
            }
        }
        if (number > 0)
        {
            bool noOutputNow = isb == 0;
            if (requiresZero is 2)
            {
                span[isb++] = digits[0];
            }
            isb += NumberToChineseCoreUnder10000(number, digits, tens, requiresZero, span[isb..], useYiShiForTen, noOutputNow && (options & ToChineseOptions.UseLiangForTwo) != 0, useLiangForHundred, useLiangForThousand);
        }
        length = isb;
    }

    private static void NumberToChineseCoreULong(ulong number, ReadOnlySpan<char> digits, ReadOnlySpan<char> tens, ReadOnlySpan<char> wans, Span<char> span, out int length, ToChineseOptions options)
    {
        if ((options & ToChineseOptions.LargeUnits) != 0)
            NumberToChineseCoreULongLargeUnits(number, digits, tens, wans, span, out length, options);
        else
            NumberToChineseCoreULongYiMax(number, digits, tens, wans[..2], span, out length, options);
    }

    private static void NumberToChineseCoreULongYiMax(ulong number, ReadOnlySpan<char> digits, ReadOnlySpan<char> tens, ReadOnlySpan<char> wans, Span<char> span, out int length, ToChineseOptions options)
    {
        Debug.Assert(wans.Length == 2);

        const int WanUnitCount = 4;

        if (number is 0)
        {
            span[0] = digits[0];
            length = 1;
            return;
        }

        var isb = 0;

        var useYiShiForTen = (options & ToChineseOptions.YiShiForTen) != 0;
        var useLiangForSingle = (options & ToChineseOptions.UseLiangForTwoWanUnits) != 0;
        var useLiangForHundred = (options & ToChineseOptions.UseLiangForTwoHundred) != 0;
        var useLiangForThousand = (options & ToChineseOptions.UseLiangForTwoThousand) != 0;

        int requiresZero = 0; // 0 no_output_now, 1 not_required, 2 required
        for (int i = WanUnitCount - 1; i >= 0; i--)
        {
            var unit = Pow10000_ULong[i];
#if NET9_0_OR_GREATER
            var (digit, rem) = Math.DivRem(number, unit);
#else
            var digit = number / unit;
            var rem = number % unit;
#endif
            if (digit > 0)
            {
                Debug.Assert(digit < 10000);
                if (requiresZero is 2)
                {
                    span[isb++] = digits[0];
                }
                isb += NumberToChineseCoreUnder10000(unchecked((uint)digit), digits, tens, requiresZero, span[isb..], useYiShiForTen, useLiangForSingle, useLiangForHundred, useLiangForThousand);
                isb += AppendWanUnit(wans, i, span[isb..]);
                requiresZero = 1;

                number = rem;
            }
            else
            {
                if (requiresZero is 1)
                {
                    // 一整个万块都是0，此处应当补零，且后续的万内数字不需要前置零
                    requiresZero = 2;
                }
            }
        }
        if (number > 0)
        {
            bool noOutputNow = isb == 0;
            Debug.Assert(number < 10000);
            if (requiresZero is 2)
            {
                span[isb++] = digits[0];
            }
            isb += NumberToChineseCoreUnder10000(unchecked((uint)number), digits, tens, requiresZero, span[isb..], useYiShiForTen, noOutputNow && (options & ToChineseOptions.UseLiangForTwo) != 0, useLiangForHundred, useLiangForThousand);
        }
        length = isb;

        // return length
        static int AppendWanUnit(ReadOnlySpan<char> wans, int wanUnit, Span<char> span)
        {
            var isb = 0;
            span[isb++] = wans[wanUnit % 2];
            var yiCount = wanUnit / 2;
            if (yiCount > 0)
            {
                span.Slice(1, yiCount).Fill(wans[1]);
                isb += yiCount;
            }
            return isb;
        }
    }

    private static void NumberToChineseCoreULongLargeUnits(ulong number, ReadOnlySpan<char> digits, ReadOnlySpan<char> tens, ReadOnlySpan<char> wans, Span<char> span, out int length, ToChineseOptions options)
    {
        if (number is 0)
        {
            span[0] = digits[0];
            length = 1;
            return;
        }

        var isb = 0;

        var useYiShiForTen = (options & ToChineseOptions.YiShiForTen) != 0;
        var useLiangForSingle = (options & ToChineseOptions.UseLiangForTwoWanUnits) != 0;
        var useLiangForHundred = (options & ToChineseOptions.UseLiangForTwoHundred) != 0;
        var useLiangForThousand = (options & ToChineseOptions.UseLiangForTwoThousand) != 0;

        int requiresZero = 0; // 0 no_output_now, 1 not_required, 2 required
        for (int i = wans.Length - 1; i >= 0; i--)
        {
            var unit = Pow10000_ULong[i];
#if NET9_0_OR_GREATER
            var (digit, rem) = Math.DivRem(number, unit);
#else
            var digit = number / unit;
            var rem = number % unit;
#endif
            if (digit > 0)
            {
                Debug.Assert(digit < 10000);
                if (requiresZero is 2)
                {
                    span[isb++] = digits[0];
                }
                isb += NumberToChineseCoreUnder10000(unchecked((uint)digit), digits, tens, requiresZero, span[isb..], useYiShiForTen, useLiangForSingle, useLiangForHundred, useLiangForThousand);
                span[isb++] = wans[i];
                requiresZero = 1;

                number = rem;
            }
            else
            {
                if (requiresZero is 1)
                {
                    // 一整个万块都是0，此处应当补零，且后续的万内数字不需要前置零
                    requiresZero = 2;
                }
            }
        }
        if (number > 0)
        {
            bool noOutputNow = isb == 0;
            Debug.Assert(number < 10000);
            if (requiresZero is 2)
            {
                span[isb++] = digits[0];
            }
            isb += NumberToChineseCoreUnder10000(unchecked((uint)number), digits, tens, requiresZero, span[isb..], useYiShiForTen, noOutputNow && (options & ToChineseOptions.UseLiangForTwo) != 0, useLiangForHundred, useLiangForThousand);
        }
        length = isb;
    }

    /// <param name="insertPrefixZero">
    /// 0 no_output_now, no prefix zero required
    /// 1 insert,        insert prefix zero if no thousand digit
    /// 2 not_insert,    do not insert prefix zero as already inserted
    /// </param>
    /// <param name="useLiangForSingle">for single digit (ones position)</param>
    /// <param name="useLiangForHundred">for hundred position</param>
    /// <param name="useLiangForThousand">for thousand position</param>
    /// <returns>written length</returns>
    private static int NumberToChineseCoreUnder10000(uint number, ReadOnlySpan<char> digits, ReadOnlySpan<char> tens, int insertPrefixZero, Span<char> span, bool useYiShiForTen, bool useLiangForSingle, bool useLiangForHundred, bool useLiangForThousand)
    {
        Debug.Assert(number < 10000);
        Debug.Assert(tens.Length <= 3);

        const int IndexLiang = 10;

        var isb = 0;
        int requiresZero = 0; // 0 prefix, 1 not_required, 2 required
        for (int i = tens.Length - 1; i >= 0; i--)
        {
            var unit = Pow10_UInt[i];
#if NET9_0_OR_GREATER
            var (digit, rem) = Math.DivRem(number, unit);
#else
            var digit = unchecked((uint)Math.DivRem(unchecked((int)number), unchecked((int)unit), out int remI));
            var rem = unchecked((uint)remI);
#endif
            if (digit > 0)
            {
                if (requiresZero == 2)
                    span[isb++] = digits[0];

                switch (digit)
                {
                    case 1:
                        if (useYiShiForTen || !(insertPrefixZero is 0 && requiresZero == 0 && i == 0))
                            span[isb++] = digits[1];
                        break;
                    case 2:
                        if (i > 0 && (i == 2 ? useLiangForThousand : useLiangForHundred)) // ten never with 两
                            span[isb++] = digits[IndexLiang];
                        else
                            span[isb++] = digits[2];
                        break;
                    default:
                        span[isb++] = digits[unchecked((int)digit)];
                        break;
                }
                span[isb++] = tens[i];

                requiresZero = 1;

                number = rem;
            }
            else
            {
                if (requiresZero != 0 || insertPrefixZero is 1)
                {
                    requiresZero = 2;
                }
            }
        }
        // 个位数
        if (number > 0)
        {
            bool noOutputNow = isb == 0;
            if (requiresZero == 2)
                span[isb++] = digits[0];
            if (useLiangForSingle && noOutputNow && number == 2)
                span[isb++] = digits[IndexLiang];
            else
                span[isb++] = digits[unchecked((int)number)];
        }
        return isb;
    }

    [Flags]
    public enum ToChineseOptions
    {
        None,
        YiShiForTen = 1 << 0,
        LargeUnits = 1 << 1,
        FinancialNumerals = 1 << 2,
        TraditionalChinese = 1 << 3,
        /// <summary>
        /// 当值为2时使用“两”
        /// </summary>
        UseLiangForTwo = 1 << 4,
        /// <summary>
        /// 使用“两百”代替“二百”
        /// </summary>
        UseLiangForTwoHundred = 1 << 5,
        /// <summary>
        /// 使用“两千”代替“二千”
        /// </summary>
        UseLiangForTwoThousand = 1 << 6,
        /// <summary>
        /// 使用“两万”“两亿”代替“二万”“二亿”，包括更高单位
        /// </summary>
        UseLiangForTwoWanUnits = 1 << 7,
        /// <summary>
        /// 在可能的情况下使用“两”代替“二”
        /// </summary>
        UseLiangForTwoAlways = UseLiangForTwo | UseLiangForTwoHundred | UseLiangForTwoThousand | UseLiangForTwoWanUnits,
    }
}