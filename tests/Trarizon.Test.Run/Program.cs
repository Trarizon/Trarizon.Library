using BenchmarkDotNet.Running;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using System.Collections;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Trarizon.Library;
using Trarizon.Library.CodeAnalysis;
using Trarizon.Library.CodeAnalysis.MemberAccess;
using Trarizon.Library.CodeGeneration;
using Trarizon.Library.Collections;
using Trarizon.Library.Collections.AllocOpt;
using Trarizon.Library.Collections.Generic;
using Trarizon.Library.RunTest.Examples;
using Trarizon.Library.Text.Json;
using Trarizon.Library.Threading;
using Trarizon.Library.Wrappers;
using Trarizon.Test.Run;

Console.WriteLine("Hello, world");

Trie<char> trie = new Trie<char>();
trie.Add("string");
trie.Add("strojdu");
trie.Add("sjjdoi");
trie.Add("tsj");

trie.Contains("string").Print();
trie.Contains("str").Print();
trie.Contains("tsjj").Print();
trie.Contains("sjjdoi").Print();

foreach (var item in trie) {
    item.Print();
}

_ = ArrayInts().Length switch
{
    0 => 1,
    _ => TraThrow.ThrowSwitchExpressionException<int>(),
};

namespace A
{
    [Singleton]
    partial class Proj
    {
        // private Proj() { }
    }
}


class Disposable : IDisposable
{
    public bool IsDisposed { get; private set; }

    public void Dispose()
    {
        IsDisposed = true;
    }
}