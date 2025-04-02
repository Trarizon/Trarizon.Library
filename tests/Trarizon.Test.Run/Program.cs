#pragma warning disable TRAEXP

using System.Collections;
using System.Collections.Specialized;
using Trarizon.Library.Collections;
using Trarizon.Test.Run;

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