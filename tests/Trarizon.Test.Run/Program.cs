#pragma warning disable TRAEXP

using CommunityToolkit.HighPerformance.Buffers;
using System.Collections;
using System.Collections.Specialized;
using Trarizon.Library.Collections;
using Trarizon.Library.Collections.Generic;
using Trarizon.Library.Wrappers;
using Trarizon.Test.Run;


var root = new N(0)
{
    Child = new N(1)
    {
        Next = new N(2)
        {
            Next = new N(3),
        },
        Child = new(4)
        {
            Next = new N(5)
            {
                Next = new N(6),
            },
            Child=new(7)
        }
    },
};

TraEnumerable.EnumerateDescendantsBreadthFirst(root, x => Optional.OfNotNull(x.Child), x => Optional.OfNotNull(x.Next), true).Print();

class N(int value)
{
    public N Child { get; set; }
    public N Next { get; set; }

    public override string ToString() => value.ToString();
}