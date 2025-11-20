/*
| Method      | Job       | Runtime   | array     | val           | cmp                  | Mean      | Error     | StdDev    | Allocated |
|------------ |---------- |---------- |---------- |-------------- |--------------------- |----------:|----------:|----------:|----------:|
| MGeneralInt | .NET 10.0 | .NET 10.0 | Int32[5]  | 2             | ?                    | 1.8865 ns | 0.0561 ns | 0.0525 ns |         - |
| MSpValInt   | .NET 10.0 | .NET 10.0 | Int32[5]  | 2             | ?                    | 0.9903 ns | 0.0301 ns | 0.0267 ns |         - |
| MGeneralInt | .NET 10.0 | .NET 10.0 | Int32[5]  | 2             | Syste(...)nt32] [66] | 1.8853 ns | 0.0393 ns | 0.0349 ns |         - |
| MSpValInt   | .NET 10.0 | .NET 10.0 | Int32[5]  | 2             | Syste(...)nt32] [66] | 1.3603 ns | 0.0339 ns | 0.0318 ns |         - |
| MGeneralObj | .NET 10.0 | .NET 10.0 | Object[4] | System.Object | ?                    | 6.6674 ns | 0.1127 ns | 0.1054 ns |         - |
| MGeneralObj | .NET 10.0 | .NET 10.0 | Object[4] | System.Object | ?                    | 6.6401 ns | 0.0923 ns | 0.0863 ns |         - |
| MSpValObj   | .NET 10.0 | .NET 10.0 | Object[4] | System.Object | ?                    | 7.0762 ns | 0.0441 ns | 0.0368 ns |         - |
| MSpValObj   | .NET 10.0 | .NET 10.0 | Object[4] | System.Object | ?                    | 7.1987 ns | 0.0916 ns | 0.0812 ns |         - |
 */
// 对int来说，特化快了1倍，甚至比直接传Default compare快
// 对object来说，通用模式快了一点，7ns到6.6ns

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Trarizon.Test.Run.HistoryBenchmarks;

[MemoryDiagnoser]
public class Benchmarks
{
    public IEnumerable<object?[]> ArgsInt()
    {
        yield return [
            new int[] { 1, 2, 3, 4, 5 },
            2,
            EqualityComparer<int>.Default,
        ];
        yield return [
            new int[] { 1, 2, 3, 4, 5 },
            2,
            null,
        ];
    }

    public IEnumerable<object?[]> ArgsObj()
    {
        var obj = new object();
        yield return [
            new object[]{new object(),obj,new object(),new object()},
            obj,
            null,
        ];
        yield return [
            new object[]{new object(),obj,new object(),new object()},
            obj,
            null,
        ];
    }

    [Benchmark]
    [ArgumentsSource(nameof(ArgsInt))]
    public int MGeneralInt(int[] array, int val, IEqualityComparer<int>? cmp)
        => MGeneral(array, val, cmp);

    [Benchmark]
    [ArgumentsSource(nameof(ArgsInt))]
    public int MSpValInt(int[] array, int val, IEqualityComparer<int>? cmp)
        => MSpVal(array, val, cmp);

    [Benchmark]
    [ArgumentsSource(nameof(ArgsObj))]
    public object? MGeneralObj(object[] array, object? val, IEqualityComparer<object?>? cmp)
        => MGeneral(array, val, cmp);

    [Benchmark]
    [ArgumentsSource(nameof(ArgsObj))]
    public object? MSpValObj(object[] array, object? val, IEqualityComparer<object?>? cmp)
        => MSpVal(array, val, cmp);

    public T MGeneral<T>(T[] array, T val, IEqualityComparer<T>? cmp)
    {
        cmp ??= EqualityComparer<T>.Default;
        foreach (var item in array) {
            if (cmp.Equals(item, val))
                return val;
        }
        return default!;
    }

    public T MSpVal<T>(T[] array, T val, IEqualityComparer<T>? cmp)
    {
        if (typeof(T).IsValueType && cmp is null) {
            foreach (var item in array) {
                if (EqualityComparer<T>.Default.Equals(item, val))
                    return val;
            }
        }
        else {
            cmp ??= EqualityComparer<T>.Default;
            foreach (var item in array) {
                if (cmp.Equals(item, val))
                    return val;
            }
        }
        return default!;
    }
}
