#pragma warning disable TRAEXP

using CommunityToolkit.HighPerformance.Buffers;
using System.Collections;
using System.Collections.Specialized;
using Trarizon.Library.Collections;
using Trarizon.Library.Collections.Generic;
using Trarizon.Test.Run;

ContiguousLinkedList<int> list = new();
var n0 = list.AddFirst(0);
list.AddFirst(1);
list.Print();
list.EnumerateEntries().Print();
var n2 = list.AddLast(2);
list.AddFirst(3);
var n4 = list.AddLast(4);
list.Print();
list.EnumerateEntries().Print();
list.RemoveFirst();
list.RemoveLast();
list.AddAfter(n0, 5);
list.Print();
list.EnumerateEntries().Print();
_ = n2.Value;
list.RemoveFirst();
list.RemoveFirst();
list.RemoveFirst();
list.RemoveFirst();
list.Print();
list.EnumerateEntries().Print();
list.AddLast(6);
list.AddFirst(7);
list.AddLast(8);
list.Print();
list.EnumerateEntries().Print();


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