using BenchmarkDotNet.Attributes;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Trarizon.Library.Collections.AllocOpt;
using Trarizon.Library.Collections.Helpers;

namespace Trarizon.Test.Run;
[MemoryDiagnoser]
public class Benchmarks
{
    public IEnumerable<object[]> ArgsSource()
    {
        yield return new IEnumerable<int>[] {
            [1,3,5,7,9,],
            [1,5,6,8,9,],
        };
        yield return new IEnumerable<int>[] {
            [1,3,5,7,9,10,10,25,68],
            [1,5,6,8,9,10,24,25,68],
        };
    }

    [Benchmark]
    [ArgumentsSource(nameof(ArgsSource))]
    public void Merge(IEnumerable<int> first, IEnumerable<int> second)
    {
        foreach (var item in first.Merge(second)) {
            _ = item;
        }
    }

    [Benchmark]
    [ArgumentsSource(nameof(ArgsSource))]
    public void MergeNew(IEnumerable<int> first, IEnumerable<int> second)
    {
        foreach (var item in first.MergeNew(second)) {
            _ = item;
        }
    }

    //[Benchmark]
    //[ArgumentsSource(nameof(ArgsSource))]
    //public void MergeFunc(IEnumerable<int> first, IEnumerable<int> second)
    //{
    //    foreach (var item in first.Merge(second, (l, r) => l <= r)) {
    //        _ = item;
    //    }
    //}

    //[Benchmark]
    //[ArgumentsSource(nameof(ArgsSource))]
    //public void MergeFuncNew(IEnumerable<int> first, IEnumerable<int> second)
    //{
    //    foreach (var item in first.MergeNew(second, (l, r) => l <= r)) {
    //        _ = item;
    //    }
    //}
}
