using System.Diagnostics;

namespace Trarizon.Library.Text;

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
        var financial = options.HasFlag(ToChineseOptions.FinancialNumerals);
        var hant = options.HasFlag(ToChineseOptions.TraditionalChinese);
        switch (financial, hant)
        {
            case (false, false):
                negative = '负';
                digits = ['零', '一', '二', '三', '四', '五', '六', '七', '八', '九'];
                tens = ['十', '百', '千'];
                wans = ['万', '亿', '兆', '京', '垓', '秭', '穰', '沟', '涧', '正', '载', '极']; // 恒河沙 阿僧祗 那由他 不可思议 无量大数
                break;
            case (false, true):
                negative = '負';
                digits = ['零', '一', '二', '三', '四', '五', '六', '七', '八', '九'];
                tens = ['十', '百', '千'];
                wans = ['萬', '億', '兆', '京', '垓', '秭', '穰', '溝', '澗', '正', '載', '極']; // 恆河沙 阿僧祇 那由他 不可思議 無量大數
                break;
            case (true, false):
                negative = '负';
                digits = ['零', '壹', '贰', '叁', '肆', '伍', '陆', '柒', '捌', '玖'];
                tens = ['拾', '佰', '仟'];
                wans = ['万', '亿', '兆', '京', '垓', '秭', '穰', '沟', '涧', '正', '载', '极'];
                break;
            case (true, true):
                negative = '負';
                digits = ['零', '壹', '貳', '參', '肆', '伍', '陸', '柒', '捌', '玖'];
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

    private static void NumberToChineseCoreUInt(uint number, ReadOnlySpan<char> digits, ReadOnlySpan<char> tens, ReadOnlySpan<char> wans, Span<char> span, out int length, ToChineseOptions options)
    {
        if (number is 0)
        {
            span[0] = digits[0];
            length = 1;
            return;
        }

        var isb = 0;

        int requiresZero = 0; // 0 no_output_now, 1 not_required, 2 required 
        for (int i = wans.Length - 1; i >= 0; i--)
        {
            var unit = Pow10000(unchecked((uint)(i + 1)));
            uint digit = number / unit;
            if (digit > 0)
            {
                if (requiresZero is 2)
                {
                    span[isb++] = digits[0];
                }
                NumberToChineseCoreUnder10000(digit, digits, tens, requiresZero, span[isb..], out length, options);
                isb += length;
                span[isb++] = wans[i];
                requiresZero = 1;
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
            number %= unit;
        }
        if (number > 0)
        {
            if (requiresZero is 2)
            {
                span[isb++] = digits[0];
            }
            NumberToChineseCoreUnder10000(number, digits, tens, requiresZero, span[isb..], out length, options);
            isb += length;
        }
        length = isb;

        static uint Pow10000(uint exponent) => exponent switch
        {
            1 => 10000u,
            2 => 100000000u,
            _ => 1,
        };
    }

    private static void NumberToChineseCoreULong(ulong number, ReadOnlySpan<char> digits, ReadOnlySpan<char> tens, ReadOnlySpan<char> wans, Span<char> span, out int length, ToChineseOptions options)
    {
        if (options.HasFlag(ToChineseOptions.LargeUnits))
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

        int requiresZero = 0; // 0 no_output_now, 1 not_required, 2 required
        for (int i = WanUnitCount - 1; i >= 0; i--)
        {
            var unit = Pow10000(unchecked((uint)(i + 1)));
            ulong digit = number / unit;
            if (digit > 0)
            {
                Debug.Assert(digit < 10000);
                if (requiresZero is 2)
                {
                    span[isb++] = digits[0];
                }
                NumberToChineseCoreUnder10000(unchecked((uint)digit), digits, tens, requiresZero, span[isb..], out length, options);
                isb += length;
                AppendWanUnit(wans, i, span[isb..], out length);
                isb += length;
                requiresZero = 1;

                number %= unit;
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
            Debug.Assert(number < 10000);
            if (requiresZero is 2)
            {
                span[isb++] = digits[0];
            }
            NumberToChineseCoreUnder10000(unchecked((uint)number), digits, tens, requiresZero, span[isb..], out length, options);
            isb += length;
        }
        length = isb;

        static ulong Pow10000(uint exponent) => exponent switch
        {
            1 => 1_0000ul,
            2 => 1_0000_0000ul,
            3 => 1_0000_0000_0000ul,
            4 => 1_0000_0000_0000_0000ul,
            _ => 1,
        };

        static void AppendWanUnit(ReadOnlySpan<char> wans, int wanUnit, Span<char> span, out int length)
        {
            var isb = 0;
            span[isb++] = wans[wanUnit % 2];
            var yiCount = wanUnit / 2;
            for (int i = 0; i < yiCount; i++)
                span[isb++] = wans[1];
            length = isb;
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

        int requiresZero = 0; // 0 no_output_now, 1 not_required, 2 required
        for (int i = wans.Length - 1; i >= 0; i--)
        {
            var unit = Pow10000(unchecked((uint)(i + 1)));
            ulong digit = number / unit;
            if (digit > 0)
            {
                Debug.Assert(digit < 10000);
                if (requiresZero is 2)
                {
                    span[isb++] = digits[0];
                }
                NumberToChineseCoreUnder10000(unchecked((uint)digit), digits, tens, requiresZero, span[isb..], out length, options);
                isb += length;
                span[isb++] = wans[i];
                requiresZero = 1;

                number %= unit;
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
            Debug.Assert(number < 10000);
            if (requiresZero is 2)
            {
                span[isb++] = digits[0];
            }
            NumberToChineseCoreUnder10000(unchecked((uint)number), digits, tens, requiresZero, span[isb..], out length, options);
            isb += length;
        }
        length = isb;

        static ulong Pow10000(uint exponent) => exponent switch
        {
            1 => 1_0000ul,
            2 => 1_0000_0000ul,
            3 => 1_0000_0000_0000ul,
            4 => 1_0000_0000_0000_0000ul,
            _ => 1,
        };
    }

    // 0 no_output_now, no prefix zero required
    // 1 insert,        insert prefix zero if no thousand digit
    // 2 not_insert,    do not insert prefix zero as already inserted
    private static void NumberToChineseCoreUnder10000(uint number, ReadOnlySpan<char> digits, ReadOnlySpan<char> tens, int insertPrefixZero, Span<char> span, out int length, ToChineseOptions options)
    {
        Debug.Assert(number < 10000);
        Debug.Assert(tens.Length <= 3);

        bool useYiShiForTen = options.HasFlag(ToChineseOptions.YiShiForTen);

        var isb = 0;
        int requiresZero = 0; // 0 prefix, 1 not_required, 2 required
        for (int i = tens.Length - 1; i >= 0; i--)
        {
            var unit = Pow10(unchecked((uint)(i + 1)));
            uint digit = number / unit;
            if (digit > 0)
            {
                if (requiresZero == 2)
                    span[isb++] = digits[0];

                if (useYiShiForTen || !(insertPrefixZero is 0 && requiresZero == 0 && i == 0 && digit == 1))
                    span[isb++] = digits[unchecked((int)digit)];
                span[isb++] = tens[i];

                requiresZero = 1;
            }
            else
            {
                if (requiresZero != 0 || insertPrefixZero is 1)
                {
                    requiresZero = 2;
                }
            }
            number %= unit;
        }
        // 个位数
        if (number > 0)
        {
            if (requiresZero == 2)
                span[isb++] = digits[0];
            span[isb++] = digits[unchecked((int)number)];
        }
        length = isb;

        static uint Pow10(uint exponent) => exponent switch
        {
            1 => 10u,
            2 => 100u,
            3 => 1000u,
            _ => 1,
        };
    }

    [Flags]
    public enum ToChineseOptions
    {
        None,
        YiShiForTen = 1 << 0,
        LargeUnits = 1 << 1,
        FinancialNumerals = 1 << 2,
        TraditionalChinese = 1 << 3,
    }
}
