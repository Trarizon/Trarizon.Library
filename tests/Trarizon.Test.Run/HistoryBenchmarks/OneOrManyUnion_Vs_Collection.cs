/*
| Method | values               | Mean      | Error     | StdDev    | Gen0   | Allocated |
|------- |--------------------- |----------:|----------:|----------:|-------:|----------:|
| Union  | Syste(...)ring] [48] | 38.518 ns | 0.5641 ns | 0.5277 ns | 0.0127 |      40 B |
| Col    | Syste(...)ring] [48] | 33.825 ns | 0.4809 ns | 0.4499 ns | 0.0127 |      40 B |
| Col    | String[1]            | 16.026 ns | 0.3532 ns | 0.3627 ns | 0.0102 |      32 B |
| Union  | String[5]            | 30.386 ns | 0.2983 ns | 0.2644 ns | 0.0102 |      32 B |
| Col    | String[5]            | 26.531 ns | 0.2865 ns | 0.2680 ns | 0.0102 |      32 B |
| Union  | str                  |  1.330 ns | 0.0140 ns | 0.0124 ns |      - |         - |
 */

using BenchmarkDotNet.Attributes;
using System.Runtime.CompilerServices;

namespace Trarizon.Test.Run.HistoryBenchmarks;
[MemoryDiagnoser]
public class OneOrManyUnion_Vs_Collection
{
    public IEnumerable<object> Args()
    {
        yield return new string[] { "a", "b", "c", "d", "e" };
        yield return "str";
        yield return Enumerable.Range(0, 5).Select(i => i.ToString()).ToList();
    }

    public IEnumerable<IEnumerable<string>> Args2()
    {
        yield return new string[] { "a", "b", "c", "d", "e" };
        yield return new string[] { "str" };
        yield return Enumerable.Range(0, 5).Select(i => i.ToString()).ToList();
    }

    [Benchmark]
    [ArgumentsSource(nameof(Args))]
    public void Union(object? values)
    {
        if (values is null)
            return;
        if (values is string v) {
            Consume(v);
        }
        else {
            var vs = Unsafe.As<IEnumerable<string>>(values)!;
            foreach (var item in vs) {
                Consume(item);
            }
        }
    }

    [Benchmark]
    [ArgumentsSource(nameof(Args2))]
    public void Col(IEnumerable<string> values)
    {
        foreach (var item in values) {
            Consume(item);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    void Consume<T>(T value)
    {
        _ = value.ToString();
    }
}
