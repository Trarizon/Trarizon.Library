using BenchmarkDotNet.Attributes;
using System.Buffers;

namespace Trarizon.Test.Run;
[MemoryDiagnoser]
public class Benchmarks
{
    private static SearchValues<char> _invalidFileNameChars = SearchValues.Create(Path.GetInvalidFileNameChars());

    private static char[] _chars = Path.GetInvalidFileNameChars();

    public IEnumerable<string[]> Args()
    {
        //yield return ["str", "asd", "ddf", "wefd", "str", "ddf"];
        yield return ["str.json"];
        yield return ["stddfsr>//$:.json"];
        yield return ["str.fasdasjson"];
        yield return ["stfsdfd/,./>.jdddson"];
    }

    [Benchmark]
    [ArgumentsSource(nameof(Args))]
    public bool Bysearch(string filename)
    {
        foreach (var c in filename) {
            if (_invalidFileNameChars.Contains(c))
                return true;
        }
        return false;
    }

    [Benchmark]
    [ArgumentsSource(nameof(Args))]
    public bool Byarr(string filename)
    {
        foreach (var c in filename) {
            if (Array.IndexOf(_chars, c) >= 0)
                return true;
        }
        return false;
    }

    [Benchmark]
    [ArgumentsSource(nameof(Args))]
    public bool Bycol(string filename)
    {
        var coll = (ICollection<char>)_chars;
        foreach (var c in filename) {
            if (coll.Contains(c))
                return true;
        }
        return false;
    }

    [Benchmark]
    [ArgumentsSource(nameof(Args))]
    public bool BySpan(string filename)
    {
        var span = _chars.AsSpan();
        foreach (var c in filename) {
            if (span.Contains(c))
                return true;
        }
        return false;
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
