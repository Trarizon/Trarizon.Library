using System.Collections;
using Trarizon.Library.Linq;

namespace Trarizon.Test.UnitTest.Linq;

[TestClass]
public class Traversable
{
    private TreeNode<int> _deepFirstTree = new TreeNode<int>(1)
    {
        new TreeNode<int>(2)
        {
            new TreeNode<int>(3)
        },
        new TreeNode<int>(4)
        {
            new TreeNode<int>(5)
            {
                new TreeNode<int>(6)
            },
            new TreeNode<int>(7)
            {
                new TreeNode<int>(8)
                {
                    new TreeNode<int>(9)
                }
            }
        },
        new TreeNode<int>(10)
    };

    private TreeNode<int> _breadthFirstTree = new TreeNode<int>(1)
    {
        new TreeNode<int>(2)
        {
            new TreeNode<int>(5)
        },
        new TreeNode<int>(3)
        {
            new TreeNode<int>(6)
            {
                new TreeNode<int>(8)
            },
            new TreeNode<int>(7)
            {
                new TreeNode<int>(9)
                {
                    new TreeNode<int>(10)
                }
            }
        },
        new TreeNode<int>(4)
    };

    [TestMethod]
    public void TraverseDeepFirst()
    {
        var tree = _deepFirstTree;

        var result = TraTraversable.TraverseDeepFirst(tree, includeSelf: true);
        Assert.AreSequenceEqual(result.Select(x => x.Value), [1, 2, 3, 4, 5, 6, 7, 8, 9, 10]);
    }

    [TestMethod]
    public void TraverseBreadthFirst()
    {
        var tree = _breadthFirstTree;

        var result = TraTraversable.TraverseBreadthFirst(tree, includeSelf: true);
        Assert.AreSequenceEqual(result.Select(x => x.Value), [1, 2, 3, 4, 5, 6, 7, 8, 9, 10]);
    }

    private class TreeNode<T>(T value) : IChildrenTraversable<TreeNode<T>>, IEnumerable<TreeNode<T>>
    {
        private readonly List<TreeNode<T>> _list = new();
        public T Value { get; } = value;

        public void Add(TreeNode<T> node) => _list.Add(node);

        public IEnumerator<TreeNode<T>> GetChildrenEnumerator() => _list.GetEnumerator();
        public IEnumerator<TreeNode<T>> GetEnumerator() => _list.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
