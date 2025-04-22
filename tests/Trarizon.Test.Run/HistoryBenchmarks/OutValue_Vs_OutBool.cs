/*
| Method        | Mean     | Error     | StdDev    | Median   | Allocated |
|-------------- |---------:|----------:|----------:|---------:|----------:|
| OutInt        | 4.486 ns | 0.0372 ns | 0.0348 ns | 4.486 ns |         - |
| ReturnInt     | 4.833 ns | 0.0355 ns | 0.0296 ns | 4.828 ns |         - |
| OutByte       | 7.975 ns | 0.0943 ns | 0.0836 ns | 7.995 ns |         - |
| ReturnByte    | 7.921 ns | 0.0900 ns | 0.0842 ns | 7.936 ns |         - |
| OutDecimal    | 1.952 ns | 0.0608 ns | 0.1296 ns | 1.890 ns |         - |
| ReturnDecimal | 1.663 ns | 0.0239 ns | 0.0224 ns | 1.660 ns |         - |
| OutString     | 5.248 ns | 0.1671 ns | 0.3703 ns | 5.346 ns |         - |
| ReturnString  | 3.837 ns | 0.1139 ns | 0.1065 ns | 3.800 ns |         - |
 */
// 这么看，out bool基本稳定比out value好，int可能是误差？
// 另外如果是void的的话还是直接return bool比较好


using BenchmarkDotNet.Attributes;
using System.Runtime.CompilerServices;

namespace Trarizon.Test.Run.HistoryBenchmarks;
[MemoryDiagnoser]
internal class OutValue_Vs_OutBool
{
    [Benchmark]
    public (int, bool) OutInt()
    {
        var exists = TryGet<int>(out var v);
        return (v, exists);
    }

    [Benchmark]
    public (int, bool) ReturnInt()
    {
        var v = GetOrDefault<int>(out var exists);
        return (v, exists);
    }

    [Benchmark]
    public (byte, bool) OutByte()
    {
        var exists = TryGet<byte>(out var v);
        return (v, exists);
    }

    [Benchmark]
    public (byte, bool) ReturnByte()
    {
        var v = GetOrDefault<byte>(out var exists);
        return (v, exists);
    }

    [Benchmark]
    public (decimal, bool) OutDecimal()
    {
        var exists = TryGet<decimal>(out var v);
        return (v, exists);
    }

    [Benchmark]
    public (decimal, bool) ReturnDecimal()
    {
        var v = GetOrDefault<decimal>(out var exists);
        return (v, exists);
    }

    [Benchmark]
    public (string, bool) OutString()
    {
        var exists = TryGet<string>(out var v);
        return (v, exists);
    }

    [Benchmark]
    public (string, bool) ReturnString()
    {
        var v = GetOrDefault<string>(out var exists);
        return (v, exists);
    }


    [MethodImpl(MethodImplOptions.NoInlining)]
    private bool TryGet<T>(out T exists)
    {
        exists = default!;
        return true;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private T GetOrDefault<T>(out bool exists)
    {
        exists = true;
        return default!;
    }
}
