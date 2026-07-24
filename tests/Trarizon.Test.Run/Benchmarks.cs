using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Trarizon.Library.Collections;
using Trarizon.Library.Collections.Generic;
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
    private const uint TestUInt = uint.MaxValue;
    private const ulong TestULong = ulong.MaxValue;

    private static readonly int[] IntValues = { 0, 1, 10, 100, 1000, 10000, 12345, 99999999, TestInt };
    private static readonly long[] LongValues = { 0, 1, 10, 10000, 100000000, 123456789012345L, TestLong };
    private static readonly uint[] UIntValues = { 0, 1, 10, 1000, 10000, 12345, 99999999, TestUInt };
    private static readonly ulong[] ULongValues = { 0, 1, 10, 10000, 100000000, 123456789012345UL, TestULong };

    [Benchmark]
    public string NumberText_UInt() => NumberText.NumberToChinese(TestUInt);

    [Benchmark]
    public string NumberText2_UInt() => NumberText2.NumberToChinese(TestUInt);

    [Benchmark]
    public string NumberText_ULong() => NumberText.NumberToChinese(TestULong);

    [Benchmark]
    public string NumberText2_ULong() => NumberText2.NumberToChinese(TestULong);

    [Benchmark]
    public int NumberText_Int_Span()
    {
        Span<char> span = stackalloc char[20];
        NumberText.NumberToChinese(TestInt, NumberText.ToChineseOptions.None, span, out int length);
        return length;
    }

    [Benchmark]
    public int NumberText2_Int_Span()
    {
        Span<char> span = stackalloc char[20];
        NumberText2.NumberToChinese(TestInt, NumberText2.ToChineseOptions.None, span, out int length);
        return length;
    }

    [Benchmark]
    public int NumberText_Long_Span()
    {
        Span<char> span = stackalloc char[40];
        NumberText.NumberToChinese(TestLong, NumberText.ToChineseOptions.None, span, out int length);
        return length;
    }

    [Benchmark]
    public int NumberText2_Long_Span()
    {
        Span<char> span = stackalloc char[40];
        NumberText2.NumberToChinese(TestLong, NumberText2.ToChineseOptions.None, span, out int length);
        return length;
    }

    // [Benchmark]
    // public void NumberText_MixedUIntValues()
    // {
    //     foreach (var value in UIntValues)
    //         NumberText.NumberToChinese(value);
    // }

    // [Benchmark]
    // public void NumberText2_MixedUIntValues()
    // {
    //     foreach (var value in UIntValues)
    //         NumberText2.NumberToChinese(value);
    // }

    // [Benchmark]
    // public void NumberText_MixedULongValues()
    // {
    //     foreach (var value in ULongValues)
    //         NumberText.NumberToChinese(value);
    // }

    // [Benchmark]
    // public void NumberText2_MixedULongValues()
    // {
    //     foreach (var value in ULongValues)
    //         NumberText2.NumberToChinese(value);
    // }
}