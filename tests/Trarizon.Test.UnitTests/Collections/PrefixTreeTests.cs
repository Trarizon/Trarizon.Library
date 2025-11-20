using FluentAssertions;
using Trarizon.Library.Collections.Generic;

namespace Trarizon.Test.UnitTests.Collections;

public class PrefixTreeTests
{
    [Fact]
    public void AddRemoveTest()
    {
        var tree = new PrefixTree<int>();

        tree.NodeCount.Should().Be(1);

        var count = 0;
        var ncount = 1;

        var node = tree.GetOrAdd([1]);
        node.Value.Should().Be(1);
        tree.NodeCount.Should().Be(ncount += 1);
        tree.Count.Should().Be(count += 1);

        tree.GetOrAdd([]);
        tree.Root.IsEnd.Should().BeTrue();
        tree.NodeCount.Should().Be(ncount);
        tree.Count.Should().Be(count += 1);

        var a = tree.GetOrAdd([1, 2, 3, 4, 5, 6]);
        tree.NodeCount.Should().Be(ncount += 5);
        tree.Count.Should().Be(count += 1);

        var b = tree.GetOrAdd([1, 2, 3, 4, 5, 6]);
        a.Should().Be(b);
        tree.NodeCount.Should().Be(ncount);
        tree.Count.Should().Be(count);

        tree.GetOrAdd([1, 2, 3, 6, 7, 8]);
        tree.NodeCount.Should().Be(ncount += 3);
        tree.Count.Should().Be(count += 1);

        tree.TryAdd([1, 2, 3, 4, 5, 6], out _).Should().BeFalse();

        tree.Remove([1, 2, 3, 4, 5]).Should().BeFalse();
        tree.NodeCount.Should().Be(ncount);
        tree.Count.Should().Be(count);

        tree.Remove([1, 2, 3, 4, 5, 6]).Should().BeTrue();
        tree.NodeCount.Should().Be(ncount -= 3);
        tree.Count.Should().Be(count -= 1);

        tree.Remove([1]).Should().BeTrue();
        tree.NodeCount.Should().Be(ncount);
        tree.Count.Should().Be(count -= 1);

        tree.Clear();
        tree.NodeCount.Should().Be(1);
        tree.Count.Should().Be(0);
    }

    [Fact]
    public void AddRemoveDictTest()
    {
        var tree = new PrefixTreeDictionary<int, string>();

        tree.NodeCount.Should().Be(1);

        var count = 0;
        var ncount = 1;

        var node = tree.GetOrAdd([1], "1");
        node.Key.Should().Be(1);
        node.Value.Should().Be("1");
        tree.NodeCount.Should().Be(ncount += 1);
        tree.Count.Should().Be(count += 1);

        node = tree.GetOrAdd([], "");
        node.Value.Should().Be("");
        tree.Root.IsEnd.Should().BeTrue();
        tree.NodeCount.Should().Be(ncount);
        tree.Count.Should().Be(count += 1);

        var a = tree.GetOrAdd([1, 2, 3, 4, 5, 6], "6");
        a.Key.Should().Be(6);
        a.Value.Should().Be("6");
        tree.NodeCount.Should().Be(ncount += 5);
        tree.Count.Should().Be(count += 1);

        var b = tree.GetOrAdd([1, 2, 3, 4, 5, 6], "5");
        b.Key.Should().Be(6);
        b.Value.Should().Be("6");
        a.Should().Be(b);
        tree.NodeCount.Should().Be(ncount);
        tree.Count.Should().Be(count);

        node = tree.GetOrAdd([1, 2, 3, 6, 7, 8], "8");
        node.Key.Should().Be(8);
        node.Value.Should().Be("8");
        tree.NodeCount.Should().Be(ncount += 3);
        tree.Count.Should().Be(count += 1);

        tree.TryAdd([1, 2, 3, 4, 5, 6], "66", out var n66).Should().BeFalse();
        n66.Key.Should().Be(6);
        n66.Value.Should().Be("6");

        tree.Remove([1, 2, 3, 4, 5]).Should().BeFalse();
        tree.NodeCount.Should().Be(ncount);
        tree.Count.Should().Be(count);

        tree.Remove([1, 2, 3, 4, 5, 6]).Should().BeTrue();
        tree.NodeCount.Should().Be(ncount -= 3);
        tree.Count.Should().Be(count -= 1);

        tree.Remove([1]).Should().BeTrue();
        tree.NodeCount.Should().Be(ncount);
        tree.Count.Should().Be(count -= 1);

        tree.Clear();
        tree.NodeCount.Should().Be(1);
        tree.Count.Should().Be(0);
    }
}
