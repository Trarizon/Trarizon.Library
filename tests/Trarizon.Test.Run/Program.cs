#pragma warning disable TRAEXP

using CommunityToolkit.HighPerformance.Buffers;
using System.Collections;
using System.Collections.Specialized;
using Trarizon.Library.Collections;
using Trarizon.Library.Collections.Generic;
using Trarizon.Test.Run;

ContiguousLinkedList<int> list = new();
list.AddFirst(0);
list.AddFirst(1);
list.AddLast(2);
list.AddFirst(3);
list.AddLast(4);
list.Print();
Queue<int> q;
q.Peek

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