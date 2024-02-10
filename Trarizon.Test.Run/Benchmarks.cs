using BenchmarkDotNet.Attributes;
using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Trarizon.Library.Collections.AllocOpt;
using Trarizon.Library.Collections.Extensions;

namespace Trarizon.Test.Run;
[MemoryDiagnoser]
public class Benchmarks
{
    public IEnumerable<object> ArgsSource()
    {
        yield return new AllocOptList<int>();
    }

    public IEnumerable<object> ArgsRefSource()
    {
        yield return new List<int>();
    }


    [Benchmark]
    [ArgumentsSource(nameof(ArgsSource))]
    public int TestVal(AllocOptList<int> list)
    {
        return Test(list);
    }

    [Benchmark]
    [ArgumentsSource(nameof(ArgsSource))]
    public int CallVal(AllocOptList<int> list)
    {
        return ((IList<int>)list).IndexOf(0);
    }

    [Benchmark]
    [ArgumentsSource(nameof(ArgsSource))]
    public int ConvertVal(AllocOptList<int> list)
    {
        return TestItf(list);
    }

    [Benchmark]
    [ArgumentsSource(nameof(ArgsRefSource))]
    public int TestRef(List<int> list)
    {
        return Test(list);
    }

    [Benchmark]
    [ArgumentsSource(nameof(ArgsRefSource))]
    public int CallRef(List<int> list)
    {
        return ((IList<int>)list).IndexOf(0);
    }

    [Benchmark]
    [ArgumentsSource(nameof(ArgsRefSource))]
    public int ConvertRef(List<int> list)
    {
        return TestItf(list);
    }

    private int Test<T>(T list) where T : IList<int>
    {
        return list.IndexOf(0);
    }

    private int TestItf(IList<int> list)
    {
        return list.IndexOf(0);
    }
}

[DebuggerDisplay("{a}, {b}, {c}")]
[StructLayout(LayoutKind.Sequential)]
public struct Entry
{
    public int a;
    public int b;
    public object c;
}