/*
| Method              | tree                 | Mean      | Error     | StdDev    | Allocated |
|-------------------- |--------------------- |----------:|----------:|----------:|----------:|
| LinkedList          | Trari(...)Char] [67] |  8.602 ns | 0.0805 ns | 0.0672 ns |         - |
| ContiguousLinkeList | Trari(...)Char] [62] | 13.577 ns | 0.1227 ns | 0.1147 ns |         - |
 */
// 上面是Tree的contains查询结果
// 也测了添加速度，以及LinkedList的两个速度
// 结果来说，Contiguous的添加比Noded快，查询反而Noded快，两者内存消耗差不多（说好的内存连续性呢
// 对LinkedList来说，Contiguous添加删除比BCL的要快10%~20%左右，内存分配差不多
// 对LinkedList来说，Contiguous的遍历很慢（BCL 20+ vs Contiguous 70+）
// 有一个可能？我这里都是一次性直接添加的，所以内存姑且还是连续的？

#pragma warning disable TRALIB

using BenchmarkDotNet.Attributes;
using Trarizon.Library.Collections.Generic;

namespace Trarizon.Test.Run.HistoryBenchmarks;
[MemoryDiagnoser]
internal class Noded_Vs_ContiguousCollection
{

    public IEnumerable<object> Args0()
    {
        var tree = new PrefixTree<char>();
        tree.TryAdd("string", out _);
        tree.TryAdd("str", out _);
        tree.TryAdd("stri", out _);
        tree.TryAdd("temp", out _);
        tree.TryAdd("foo", out _);
        tree.TryAdd("foo", out _);
        tree.TryAdd("foobar", out _);
        tree.TryAdd("", out _);
        yield return tree;
    }

    public IEnumerable<object> Args1()
    {
        var tree = new ContiguousPrefixTree<char>();
        tree.TryAdd("string", out _);
        tree.TryAdd("str", out _);
        tree.TryAdd("stri", out _);
        tree.TryAdd("temp", out _);
        tree.TryAdd("foo", out _);
        tree.TryAdd("foo", out _);
        tree.TryAdd("foobar", out _);
        tree.TryAdd("", out _);
        yield return tree;
    }


    [Benchmark]
    [ArgumentsSource(nameof(Args0))]
    public void LinkedList(PrefixTree<char> tree)
    {
        tree.Contains("string");
    }

    [Benchmark]
    [ArgumentsSource(nameof(Args1))]
    public void ContiguousLinkeList(ContiguousPrefixTree<char> tree)
    {
        tree.Contains("string");
    }

}
