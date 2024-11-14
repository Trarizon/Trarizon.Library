using BenchmarkDotNet.Attributes;
using System.Buffers;
using Trarizon.Library.Collections;
using Trarizon.Library.Wrappers;

namespace Trarizon.Test.Run;
[MemoryDiagnoser]
public class Benchmarks
{
    private static SearchValues<char> _invalidFileNameChars = SearchValues.Create(Path.GetInvalidFileNameChars());

    private static char[] _chars = Path.GetInvalidFileNameChars();

    public IEnumerable<string[]> Args()
    {
        //yield return ["str", "asd", "ddf", "wefd", "str", "ddf"];
        yield return ArrayValues(i => i.ToString(), 32);
    }

    public IEnumerable<StringSplitOptions[]> Args2()
    {
        //yield return ["str", "asd", "ddf", "wefd", "str", "ddf"];
        yield return ArrayValues(i => (StringSplitOptions)i, 32);
    }

    static void Consume<T>(T val) => val?.ToString();

    static Disposable _dis = new();

    [Benchmark]
    [ArgumentsSource(nameof(Args))]
    public void ByDefault(string[] arr)
    {
        arr.AsSpan().ContainsByComparer("str");
    }

    [Benchmark]
    [ArgumentsSource(nameof(Args))]
    public void Bycustom(string[] arr)
    {
        arr.AsSpan().ContainsByComparer("str", EqualityComparer<string>.Default);
    }

    [Benchmark]
    [ArgumentsSource(nameof(Args2))]
    public void ByDefault(StringSplitOptions[] arr)
    {
        arr.AsSpan().ContainsByComparer(StringSplitOptions.RemoveEmptyEntries);
    }

    [Benchmark]
    [ArgumentsSource(nameof(Args2))]
    public void Bycustom(StringSplitOptions[] arr)
    {
        arr.AsSpan().ContainsByComparer(StringSplitOptions.RemoveEmptyEntries, EqualityComparer<StringSplitOptions>.Default);
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
