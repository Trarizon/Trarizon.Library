#pragma warning disable TRAEXP

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Trarizon.Library.CodeAnalysis;
using Trarizon.Library.CodeGeneration;
using Trarizon.Library.Collections;
using Trarizon.Library.Collections.AllocOpt;
using Trarizon.Library.Collections.Generic;
using Trarizon.Test.Run;

//TraAlgorithm.LevenshteinDistance("apple", "rad").Print();

EnumerateInts(0).Intersperse(9).Print(); ;
ArrayInts(1).Intersperse(9).Print(); ;

//PrefixTree<char> t = new();

//t.GetOrAdd("string");
//t.GetOrAdd("stringnext");
//t.GetOrAdd("bool");
//t.GetOrAdd("bolt");
//t.GetOrAdd("str");
//t.GetOrAdd("");

//Print();

//t.Root.IsEnd.Print();

//t.Remove("stringnext").Print();
//Print();

//t.Remove("").Print();

//Print();
//t.Root.IsEnd.Print();

void Print()
{
    //TraEnumerable.EnumerateLeveledChildCollection(t.RootNode, n => n.Children)
    //    .Select(tpl => (tpl.Level, tpl.Items.Select(node => (node.Value, node.IsEnd))))
    //    .Print();
    //TraEnumerable.EnumerateLeveledDescendants(t.RootNode, n => n.Children)
    //    .Select(tpl => (tpl.Level, tpl.Item.Value, tpl.Item.IsEnd))
    //    .Print();
}

//RunBenchmarks();
[Singleton]
partial class D
{
    private D()
    {
        Console.WriteLine(1);
    }
}

class A
{
    [FriendAccess(typeof(B))]
    public string Name { get; set; }
}

class B
{
    public static string Name => new A().Name;
}

class C
{
    //public static string Name => new A().Name;
}