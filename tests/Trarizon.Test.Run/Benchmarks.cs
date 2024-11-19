using BenchmarkDotNet.Attributes;
using System.Buffers;
using System.Runtime.CompilerServices;
using Trarizon.Library.Collections;
using Trarizon.Library.Wrappers;

namespace Trarizon.Test.Run;
[MemoryDiagnoser]
public class Benchmarks
{
    private static SearchValues<char> _invalidFileNameChars = SearchValues.Create(Path.GetInvalidFileNameChars());

    private static char[] _chars = Path.GetInvalidFileNameChars();

    public IEnumerable<IEnumerable<string>> Args()
    {
        yield return EnumerateCollection("str", "str", null, "val", "va", "str", null, "val", "rig", "v", "rig")!;
    }

    //[MethodImpl(MethodImplOptions.NoInlining)]
    static void Consume<T>(T val) => _ = val;


    [Benchmark]
    [ArgumentsSource(nameof(Args))]
    public void ByFor(IEnumerable<string> strs)
    {
        foreach (var item in strs.DuplicatesBy(str => str)) {

        }
    }

    [Benchmark]
    [ArgumentsSource(nameof(Args))]
    public void ByForeach(IEnumerable<string> strs)
    {
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
