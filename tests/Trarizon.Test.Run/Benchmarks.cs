using BenchmarkDotNet.Attributes;
using System.Runtime.InteropServices;
using Trarizon.Library.Collections;

namespace Trarizon.Test.Run;
[MemoryDiagnoser]
public class Benchmarks
{
    public IEnumerable<string[]> Args()
    {
        yield return ["str", "asd", "ddf", "wefd", "str", "ddf"];
    }

    [Benchmark]
    [ArgumentsSource(nameof(Args))]
    public void ByDict(IEnumerable<string> args)
    {
        IEnumerable<string> Iter()
        {
            var dict = new Dictionary<string, bool>();
            foreach (var arg in args) {
                if (AddOrUpdate(dict, arg))
                    yield return arg;
            }
        }

        bool AddOrUpdate(Dictionary<string, bool> dict, string arg)
        {
            ref var value = ref CollectionsMarshal.GetValueRefOrAddDefault(dict, arg, out var exists);
            if (exists) {
                if (!value) {
                    value = true;
                    return true;
                }
            }
            else {
                value = false;
            }
            return false;
        }
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
