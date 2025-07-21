using BenchmarkDotNet.Attributes;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Trarizon.Library.Collections;
using Trarizon.Library.Collections.Generic;
using Trarizon.Library.Wrappers;

#pragma warning disable TRALIB

namespace Trarizon.Test.Run;
[MemoryDiagnoser]
public class Benchmarks
{
    private static SearchValues<char> _invalidFileNameChars = SearchValues.Create(Path.GetInvalidFileNameChars());

    private static char[] _chars = Path.GetInvalidFileNameChars();

    //public IEnumerable<object> Args0()
    //{
    //    var tree = new PrefixTree<char>();
    //    tree.TryAdd("string", out _);
    //    tree.TryAdd("str", out _);
    //    tree.TryAdd("stri", out _);
    //    tree.TryAdd("temp", out _);
    //    tree.TryAdd("foo", out _);
    //    tree.TryAdd("foo", out _);
    //    tree.TryAdd("foobar", out _);
    //    tree.TryAdd("", out _);
    //    yield return tree;
    //}

    //public IEnumerable<object> Args1()
    //{
    //    var tree = new ContiguousPrefixTree<char>();
    //    tree.TryAdd("string", out _);
    //    tree.TryAdd("str", out _);
    //    tree.TryAdd("stri", out _);
    //    tree.TryAdd("temp", out _);
    //    tree.TryAdd("foo", out _);
    //    tree.TryAdd("foo", out _);
    //    tree.TryAdd("foobar", out _);
    //    tree.TryAdd("", out _);
    //    yield return tree;
    //}


    //[Benchmark]
    //[ArgumentsSource(nameof(Args0))]
    //public void LinkedList(PrefixTree<char> tree)
    //{
    //    tree.Contains("string");
    //}

    //[Benchmark]
    //[ArgumentsSource(nameof(Args1))]
    //public void ContiguousLinkeList(ContiguousPrefixTree<char> tree)
    //{
    //    tree.Contains("string");
    //}
}
