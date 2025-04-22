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

    public IEnumerable<object[]> Args()
    {
        yield return [new string[100], new string[100]];
    }

    public IEnumerable<object[]> ArgsI()
    {
        yield return [new int[100], new int[100]];
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static void Consume<T>(T val) => _ = val;


    static void Invoke(Action<string> action, string str)
    {
        action?.Invoke(str);
    }

    [Benchmark]
    public void Out()
    {
        GetOrDefault(out var v);
        v.ToString();
    }

    [Benchmark]
    public void ReturnInt()
    {
        TryGet().ToString();
    }


    [MethodImpl(MethodImplOptions.NoInlining)]
    private bool TryGet()
    {
        return true;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void GetOrDefault(out bool exists)
    {
        exists = true;
        return;
    }
}
