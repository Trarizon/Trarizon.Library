using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Trarizon.Library.Collections.Extensions;
using Trarizon.Library.Collections.Extensions.Query;

namespace Trarizon.Test.Run;
[MemoryDiagnoser]
public class Benchmarks
{
    public IEnumerable<object[]> ArgsSource()
    {
        int v = 1;
        for (int i = 0; i < 2; i++)
            yield return [v <<= 2, 5];
    }

    [Benchmark]
    [ArgumentsSource(nameof(ArgsSource))]
    public void MarshalInt(int count, int val)
    {
        var list = new List<int>();
        CollectionsMarshal.SetCount(list, count);
        list.Fill(val);
    }

    [Benchmark]
    [ArgumentsSource(nameof(ArgsSource))]
    public void AddInt(int count, int val)
    {
        var list = new List<int>();
        for (int i = 0; i < count; i++)
            list.Add(val);
    }

    [Benchmark]
    [ArgumentsSource(nameof(ArgsSource))]
    public void MarshalObj(int count, object input)
    {
        var list = new List<object>();
        CollectionsMarshal.SetCount(list, count);
        list.Fill(input);
    }

    [Benchmark]
    [ArgumentsSource(nameof(ArgsSource))]
    public void AddObj(int count, object input)
    {
        var list = new List<object>();
        for (int i = 0; i < count; i++)
            list.Add(input);
    }
}
