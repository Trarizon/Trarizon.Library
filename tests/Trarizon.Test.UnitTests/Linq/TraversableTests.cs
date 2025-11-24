using FluentAssertions;
using System.Collections;
using Trarizon.Library.Linq;

namespace Trarizon.Test.UnitTests.Linq;

public class TraversableTests
{
    [Theory]
    [MemberData(nameof(BreadthFirstTree))]
    public void TraverseBreadthFirst(TreeNode<int> tree)
    {
        tree.TraverseBreadthFirst(includeSelf: true).Select(x => x.Value)
            .Should().Equal(1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
        new TreeNodeWrap<int>(tree).TraverseBreadthFirst().Select(x => x.Value)
            .Should().Equal(2, 3, 4, 5, 6, 7, 8, 9, 10);
        tree.Children.TraverseDescendantsBreadthFirst(x => x.Children).Select(x => x.Value)
            .Should().Equal(2, 3, 4, 5, 6, 7, 8, 9, 10);
        TraTraversable.TraverseDescendantsBreadthFirst(tree, x => x.Children, includeSelf: true).Select(x => x.Value)
            .Should().Equal(1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
    }

    [Theory]
    [MemberData(nameof(DepthFirstTree))]
    public void TraverseDepthFirst(TreeNode<int> tree)
    {
        tree.TraverseDepthFirst(includeSelf: true).Select(x => x.Value)
            .Should().Equal(1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
        new TreeNodeWrap<int>(tree).TraverseDepthFirst().Select(x => x.Value)
            .Should().Equal(2, 3, 4, 5, 6, 7, 8, 9, 10);
        tree.Children.TraverseDescendantsDepthFirst(x => x.Children).Select(x => x.Value)
            .Should().Equal(2, 3, 4, 5, 6, 7, 8, 9, 10);
        TraTraversable.TraverseDescendantsDepthFirst(tree, x => x.Children, includeSelf: true).Select(x => x.Value)
            .Should().Equal(1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
    }

    public static TheoryData<TreeNode<int>> DepthFirstTree => new(new TreeNode<int>(1)
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
    });

    public static TheoryData<TreeNode<int>> BreadthFirstTree => new(new TreeNode<int>(1)
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
    });

    public class TreeNode<T>(T value) : IChildrenTraversable<TreeNode<T>>, IEnumerable<TreeNode<T>>
    {
        private readonly List<TreeNode<T>> _list = new();
        public T Value { get; } = value;

        public IEnumerable<TreeNode<T>> Children => _list;

        public void Add(TreeNode<T> node) => _list.Add(node);

        public IEnumerator<TreeNode<T>> GetChildrenEnumerator() => _list.GetEnumerator();
        public IEnumerator<TreeNode<T>> GetEnumerator() => _list.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class TreeNodeWrap<T>(TreeNode<T> node) : IChildrenTraversable<TreeNode<T>>
    {
        public IEnumerator<TreeNode<T>> GetChildrenEnumerator() => node.GetChildrenEnumerator();
    }
}
