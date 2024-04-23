using BenchmarkDotNet.Attributes;
using Trarizon.Library.Collections.Helpers;
using Trarizon.Library.Helpers;

namespace Trarizon.Test.Run;
[MemoryDiagnoser]
public class Benchmarks
{
    public IEnumerable<long> ArgsSource()
    {
        yield return 1;
    }

    public IEnumerable<string[]> Args()
    {
        yield return ["str", "asd", "ddf", "wefd"];
    }

    [Benchmark]
    [ArgumentsSource(nameof(ArgsSource))]
    public ulong Sort(long ints)
    {
        ref readonly var val = ref UnsafeHelper.AsReadOnly<long, ulong>(ref ints);
        return val;
    }

    [Benchmark]
    [ArgumentsSource(nameof(ArgsSource))]
    public ulong SortComparer(long ints)
    {
        ref readonly var val = ref UnsafeHelper.AsReadOnlyInline<long, ulong>(ref ints);
        return val;
    }
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
