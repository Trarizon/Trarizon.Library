#pragma warning disable TRAEXP

using Trarizon.Library.Collections;
using Trarizon.Library.Collections.Generic;
using Trarizon.Test.Run;


PrefixTree<char> t = new();

t.Add("string");
t.Add("stringnext");
t.Add("bool");
t.Add("bolt");
t.Add("str");
t.Add("");

Print();

t.RootNode.IsEnd.Print();

t.Remove("stringnext").Print();
Print();

t.Remove("").Print();

Print();
t.RootNode.IsEnd.Print();

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