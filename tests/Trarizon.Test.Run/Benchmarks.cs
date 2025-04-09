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
    [ArgumentsSource(nameof(ArgsI))]
    public void Arr(int[] src, int[] dst)
    {
        Array.Copy(src, dst, src.Length);
        //Invoke(new Class().ExtensionInvoke, "B");

    }

    [Benchmark]
    [ArgumentsSource(nameof(ArgsI))]
    public void Span(int[] src, int[] dst)
    {
        src.AsSpan().CopyTo(dst);
        //Class c = new();
        //Invoke(str => c.s=str, "C");

    }

    [Benchmark]
    [ArgumentsSource(nameof(Args))]
    public void ArrStr(string[] src, string[] dst)
    {
        Array.Copy(src, dst, src.Length);
    }

    [Benchmark]
    [ArgumentsSource(nameof(Args))]
    public void SpanStr(string[] src, string[] dst)
    {
        src.AsSpan().CopyTo(dst);
        //Class c = new();
        //Invoke(str => c.s=str, "C");

    }
}
