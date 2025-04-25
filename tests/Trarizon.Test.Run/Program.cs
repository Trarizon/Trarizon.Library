#pragma warning disable TRAEXP

using Trarizon.Library.Collections;
using Trarizon.Library.Collections.Specialized;
using Trarizon.Test.Run;


PrefixTree<char> t = new();

t.GetOrAdd("string");
t.GetOrAdd("stringnext");
t.GetOrAdd("bool");
t.GetOrAdd("bolt");
t.GetOrAdd("str");
t.GetOrAdd("");

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