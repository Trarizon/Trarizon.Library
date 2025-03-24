#pragma warning disable TRAEXP

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
using Trarizon.Library.Buffers.Pooling;
using Trarizon.Library.CodeAnalysis;
using Trarizon.Library.CodeAnalysis.MemberAccess;
using Trarizon.Library.CodeGeneration;
using Trarizon.Library.Collections;
using Trarizon.Library.Collections.AllocOpt;
using Trarizon.Library.Collections.Generic;
using Trarizon.Library.Collections.Specialized;
using Trarizon.Library.Numerics;
using Trarizon.Library.RunTest.Examples;
using Trarizon.Library.Text;
using Trarizon.Library.Text.Json;
using Trarizon.Library.Threading;
using Trarizon.Library.Wrappers;
using Trarizon.Test.Run;

var list=new ContiguousLinkedList<int>();
list.AddLast(1);
list.AddLast(2);
list.Print();
list.AddAfter(list.FirstNode, 3);
list.Print();
list.AddFirst(4);
list.Print();
list.Remove(list.FirstNode.Next);
list.Print();

class C : IEnumerable<C>
{
    public int Value { get; set; }
    public List<C> List { get; set; } = [];

    public C(int value)
    {
        Value = value;
    }

    public void Add(C c)
    {
        List.Add(c);
    }

    public IEnumerator<C> GetEnumerator() => List.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}