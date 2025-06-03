#pragma warning disable TRAEXP

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using BenchmarkDotNet.Filters;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Trarizon.Library.CodeAnalysis;
using Trarizon.Library.CodeGeneration;
using Trarizon.Library.Collections;
using Trarizon.Library.Collections.AllocOpt;
using Trarizon.Library.Collections.Generic;
using Trarizon.Library.Components;
using Trarizon.Test.Run;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;


Console.WriteLine("ready");
Console.Read();

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
