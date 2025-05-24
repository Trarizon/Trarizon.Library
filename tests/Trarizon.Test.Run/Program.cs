#pragma warning disable TRAEXP

using BenchmarkDotNet.Filters;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Trarizon.Library.CodeAnalysis;
using Trarizon.Library.CodeGeneration;
using Trarizon.Library.Collections;
using Trarizon.Library.Collections.AllocOpt;
using Trarizon.Library.Collections.Generic;
using Trarizon.Library.Components;
using Trarizon.Test.Run;

//TraAlgorithm.LevenshteinDistance("apple", "rad").Print();

using var fsw = new FileSystemWatcher(@"D:\Pictures")
{
    Path = @"D:\Pictures",
    NotifyFilter = NotifyFilters.Attributes
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Security
                                 | NotifyFilters.Size,
    EnableRaisingEvents = true,
    IncludeSubdirectories = true,
};

string? prevDel = null;

fsw.Changed += (s, e) =>
{
    (e.ChangeType, e.FullPath).Print();
};

fsw.Created += (s, e) =>
{
    if (prevDel != null
        && Path.GetFileName(e.FullPath) == Path.GetFileName(prevDel)) {
        ("Move", e.FullPath).Print();
    }
    else {
        (e.ChangeType, e.FullPath).Print();
    }
    prevDel = null;
};

fsw.Deleted += (s, e) =>
{
    prevDel = e.FullPath;
    (e.ChangeType, e.FullPath).Print();
};

fsw.Renamed += (s, e) =>
{
    (e.ChangeType, e.FullPath).Print();
};

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
