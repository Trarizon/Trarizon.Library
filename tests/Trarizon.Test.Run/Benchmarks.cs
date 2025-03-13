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

    [MethodImpl(MethodImplOptions.NoInlining)]
    static void Consume<T>(T val) => _ = val;


    static void Invoke(Action<string> action, string str)
    {
        action?.Invoke(str);
    }

    [Benchmark]
    public void Ext()
    {
        //Invoke(new Class().ExtensionInvoke, "B");

    }

    [Benchmark]
    public void Lamb()
    {
        //Class c = new();
        //Invoke(str => c.s=str, "C");

    }
}
