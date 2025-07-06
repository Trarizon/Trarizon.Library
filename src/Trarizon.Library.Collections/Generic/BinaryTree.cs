using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Trarizon.Library.Collections.Helpers;

namespace Trarizon.Library.Collections.Generic;
internal enum BinaryTreeNavigation
{
    Parent, LeftChild, RightChild,
}

internal class BinaryTree<T>
{
    private int _count;
    private int _version;

    public Node? Root { get; private set; }

    [MemberNotNull(nameof(Root))]
    public Node SetRoot(T value)
    {
        if (Root is null) {
            Root = new(this, value);
        }
        else {
            Root.Value = value;
        }
        return Root;
    }

    /// <summary>
    /// Add a node of <paramref name="value"/> to <paramref name="node"/> according to <paramref name="navigation"/>, 
    /// and adjust new child of <paramref name="value"/> node according to <paramref name="childNavigation"/>
    /// </summary>
    /// <param name="navigation">The relationship of <paramref name="value"/> from <paramref name="node"/></param>
    /// <param name="childNavigation">Where to put the new child of <paramref name="value"/></param>
    public Node Add(Node node, T value, BinaryTreeNavigation navigation, BinaryTreeNavigation childNavigation = BinaryTreeNavigation.LeftChild)
    {
        ValidateNodeBelonging(node);

        var @new = new Node(this, value);
        switch (navigation) {
            case BinaryTreeNavigation.Parent: {
                if (node != Root) {
                    Debug.Assert(node._parent is not null);
                    var parent = node._parent!;
                    @new._parent = parent;
                    if (parent._left == node) {
                        parent._left = @new;
                    }
                    else {
                        Debug.Assert(parent._right == node);
                        parent._right = @new;
                    }
                }
                else {
                    Root = @new;
                }

                ConcatChild(@new, node, childNavigation);
                break;
            }
            case BinaryTreeNavigation.LeftChild: {
                var original = node._left;
                node._left = @new;
                @new._parent = node;
                if (original is not null)
                    ConcatChild(@new, original, childNavigation);
                break;
            }
            case BinaryTreeNavigation.RightChild: {
                var original = node._right;
                node._right = @new;
                @new._parent = node;
                if (original is not null)
                    ConcatChild(@new, original, childNavigation);
                break;
            }
            default:
                Throws.ThrowInvalidOperation("Unknown navigation.");
                break;
        }

        _count++;
        _version++;
        return @new;

        static void ConcatChild(Node parent, Node child, BinaryTreeNavigation childNavigation)
        {
            child._parent = parent;
            switch (childNavigation) {
                case BinaryTreeNavigation.LeftChild:
                    parent._left = child;
                    break;
                case BinaryTreeNavigation.RightChild:
                    parent._right = child;
                    break;
                default:
                    Throws.ThrowInvalidOperation("Unknown child navigation.");
                    break;
            }
        }
    }

    private void ValidateNodeBelonging(Node node)
    {
        if (node._tree != this)
            Throws.ThrowInvalidOperation("Node does not belong to this BinaryTree.");
    }

    public sealed class Node
    {
        internal readonly BinaryTree<T> _tree;
        internal Node? _parent;
        internal Node? _left;
        internal Node? _right;
        private T _value;

        internal Node(BinaryTree<T> tree, T value)
        {
            _tree = tree;
            _value = value;
        }

        public Node? Parent => _parent;
        public Node? LeftChild => _left;
        public Node? RightChild => _right;

        public T Value { get => _value; set => _value = value; }

        public ref T ValueRef => ref _value;

        public bool IsLeaf
        {
            get {
                Debug.Assert((_left is null && _right is null) || _left != _right);
                return _left == _right;
            }
        }
    }
}
