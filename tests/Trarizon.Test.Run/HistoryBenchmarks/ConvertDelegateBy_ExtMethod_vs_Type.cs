using BenchmarkDotNet.Attributes;
using Trarizon.Library.Collections.Helpers;

namespace Trarizon.Test.Run;
// 对比了通过扩展方法修改delegate定义和通过类型修改
// 结论是扩展方法全方面辣鸡.jpg，内存分配64B
[MemoryDiagnoser]
public class ConvertDelegateBy_ExtMethod_vs_Type
{
    public IEnumerable<int[]> ArgsSource()
    {
        yield return [1, 9, 8, 4, 3, 5, 4,];
    }

    public IEnumerable<string[]> Args()
    {
        yield return ["str", "asd", "ddf", "wefd"];
    }

    [Benchmark]
    [ArgumentsSource(nameof(ArgsSource))]
    public void Sort(int[] ints)
    {
        ints.AsSpan().SortStably((str, str2) => str.CompareTo(str2));
    }

    [Benchmark]
    [ArgumentsSource(nameof(ArgsSource))]
    public void SortComparer(int[] ints)
    {
        ints.AsSpan().SortStably();
    }

    //[Benchmark]
    //[ArgumentsSource(nameof(ArgsSource))]
    //public void SortNew(int[] ints)
    //{
    //    ints.AsSpan().SortStablyWithExtMethod((str, str2) => str.CompareTo(str2));
    //}

    [Benchmark]
    [ArgumentsSource(nameof(Args))]
    public void Sort(string[] strings)
    {
        strings.AsSpan().SortStably((str, str2) => str.CompareTo(str2));
    }

    [Benchmark]
    [ArgumentsSource(nameof(Args))]
    public void SortComparer(string[] strings)
    {
        strings.AsSpan().SortStably();
    }

    //[Benchmark]
    //[ArgumentsSource(nameof(Args))]
    //public void SortNew(string[] strings)
    //{
    //    strings.AsSpan().SortStablyWithExtMethod((str, str2) => str.CompareTo(str2));
    //}
}


//public static void SortStablyWithExtMethod<T>(this Span<T> span, Comparison<T>? comparison = null)
//{
//    var keys = ArrayPool<(int, T)>.Shared.Rent(span.Length);
//    try {
//        comparison ??= Comparer<T>.Default.Compare;
//        for (int i = 0; i < span.Length; i++)
//            keys[i] = (i, span[i]);
//        keys.AsSpan(0, span.Length).Sort(comparison.ToStable);
//    } finally {
//        ArrayPool<(int, T)>.Shared.Return(keys);
//    }
//}

//private static int ToStable<T>(this Comparison<T> comparison, (int, T) x, (int, T) y)
//{
//    var result = comparison(x.Item2, y.Item2);
//    return result != 0 ? result : x.Item1.CompareTo(y.Item1);
//}
