using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Trarizon.Library.Collections.Helpers;

namespace Trarizon.Library.Collections.Generic;

public partial class PrefixTree<T>
{
    internal Node? _root;
    internal int _count;
    private int _nodeCount;
    internal int _version;
    private readonly IEqualityComparer<T>? _comparer;

    public Node Root
    {
        get {
            if (_root is null) {
                _root = new(this, default);
            }
            return _root;
        }
    }

    public int NodeCount => _nodeCount;

    public int Count => _count;

    public IEqualityComparer<T> Comparer => _comparer ?? EqualityComparer<T>.Default;

    public PrefixTree(IEqualityComparer<T>? comparer = null)
    {
        if (!typeof(T).IsValueType)
            _comparer = comparer ?? EqualityComparer<T>.Default;
        else if (comparer is not null && !ReferenceEquals(comparer, EqualityComparer<T>.Default))
            _comparer = comparer;
        _nodeCount = 1;
    }

    public bool TryGetNode(ReadOnlySpan<T> prefixSequence, [MaybeNullWhen(false)] out Node node)
    {
        node = FindPrefixNode(prefixSequence);
        return node is not null;
    }

    public bool TryGetNode(IEnumerable<T> prefixSequence, [MaybeNullWhen(false)] out Node node)
    {
        node = FindPrefixNode(prefixSequence);
        return node is not null;
    }

    public bool Contains(ReadOnlySpan<T> sequence)
        => FindPrefixNode(sequence) is { IsEnd: true };

    public bool Contains(IEnumerable<T> sequence)
        => FindPrefixNode(sequence) is { IsEnd: true };

    public bool ContainsPrefix(ReadOnlySpan<T> prefixSequence)
        => FindPrefixNode(prefixSequence) is not null;

    public bool ContainsPrefix(IEnumerable<T> prefixSequence)
        => FindPrefixNode(prefixSequence) is not null;

    private Node? FindPrefixNode(ReadOnlySpan<T> prefix)
    {
        if (prefix.IsEmpty)
            return Root;

        if (_root is null)
            return null;

        var node = _root;

        if (typeof(T).IsValueType && _comparer is null) {
            foreach (var val in prefix) {
                var children = node.ChildrenSpan;
                foreach (var child in children) {
                    if (EqualityComparer<T>.Default.Equals(child.Value!, val)) {
                        node = child;
                        goto ContinueFor;
                    }
                }
                return null;

            ContinueFor:
                continue;
            }
        }
        else {
            Debug.Assert(_comparer is not null);
            var comparer = _comparer!;
            foreach (var val in prefix) {
                var children = node.ChildrenSpan;
                foreach (var child in children) {
                    if (comparer.Equals(child.Value!, val)) {
                        node = child;
                        goto ContinueFor;
                    }
                }
                return null;

            ContinueFor:
                continue;
            }
        }

        return node;
    }

    private Node? FindPrefixNode(IEnumerable<T> prefix)
    {
        if (_root is null)
            return null;

        var node = _root;

        if (typeof(T).IsValueType && _comparer is null) {
            foreach (var val in prefix) {
                var children = node.ChildrenSpan;
                foreach (var child in children) {
                    if (EqualityComparer<T>.Default.Equals(child.Value!, val)) {
                        node = child;
                        goto ContinueFor;
                    }
                }
                return null;

            ContinueFor:
                continue;
            }
        }
        else {
            Debug.Assert(_comparer is not null);
            var comparer = _comparer!;
            foreach (var val in prefix) {
                var children = node.ChildrenSpan;
                foreach (var child in children) {
                    if (comparer.Equals(child.Value!, val)) {
                        node = child;
                        goto ContinueFor;
                    }
                }
                return null;

            ContinueFor:
                continue;
            }
        }
        return node;
    }

    public Node GetOrAdd(ReadOnlySpan<T> sequence)
    {
        TryAdd(sequence, out var node);
        return node;
    }

    public Node GetOrAdd(IEnumerable<T> sequence)
    {
        TryAdd(sequence, out var node);
        return node;
    }

    public bool TryAdd(ReadOnlySpan<T> sequence, out Node node)
    {
        node = Root;
        foreach (var item in sequence) {
            node = GetOrAddChild(node, item);
        }

        if (node._end) {
            return false;
        }

        _version++;
        _count++;
        node._end = true;
        return true;
    }

    public bool TryAdd(IEnumerable<T> sequence, out Node node)
    {
        node = Root;
        foreach (var item in sequence) {
            node = GetOrAddChild(node, item);
        }

        if (node._end) {
            return false;
        }

        _version++;
        _count++;
        node._end = true;
        return true;
    }

    private Node GetOrAddChild(Node parent, T value)
    {
        Debug.Assert(parent._tree == this);

        if (typeof(T).IsValueType && _comparer is null) {
            foreach (var child in parent.ChildrenSpan) {
                if (EqualityComparer<T>.Default.Equals(child.Value!, value)) {
                    return child;
                }
            }
        }
        else {
            Debug.Assert(_comparer is not null);
            var comparer = _comparer!;
            foreach (var child in parent.ChildrenSpan) {
                if (comparer.Equals(child.Value!, value)) {
                    return child;
                }
            }
        }

        var node = new Node(this, value);
        parent.AddChildAndSetParent(node);
        _nodeCount++;
        return node;
    }

    public bool Remove(ReadOnlySpan<T> sequence)
    {
        var node = FindPrefixNode(sequence);
        if (node is not { IsEnd: true })
            return false;

        RemoveEndNode(node);
        _count--;
        _version++;
        return true;
    }

    public bool Remove(IEnumerable<T> sequence)
    {
        var node = FindPrefixNode(sequence);
        if (node is not { IsEnd: true })
            return false;

        RemoveEndNode(node);
        _count--;
        _version++;
        return true;
    }

    internal void RemoveEndNode(Node endNode)
    {
        Debug.Assert(endNode._tree == this && endNode._end);
        Debug.Assert(_root is not null, "If there's any node in the tree, the root node should have been initialized");

        endNode._end = false;
        if (endNode.ChildrenSpan.IsEmpty) {
            // If this is leaf, remove redundant nodes on this branch
            var node = endNode;
            do {
                Debug.Assert(node._parent is not null);
                var parent = node._parent!;
                parent.RemoveChild(node);
                _nodeCount--;
                node = parent;
            } while (node != _root && node.ChildrenSpan.IsEmpty && !node.IsEnd);
        }
    }

    public void Clear()
    {
        _count = 0;
        _nodeCount = 1;
        _root?.ClearChildren();
        _version++;
    }

    public sealed class Node
    {
        internal PrefixTree<T> _tree;
        internal Node? _parent;
        private Node[] _children;
        private int _childCount = 0;
        private readonly T? _value;
        internal bool _end = false;

        internal Node(PrefixTree<T> tree, T? value)
        {
            _tree = tree;
            _children = [];
            _value = value;
        }

        public Node? Parent => _parent;

        public T? Value => _value;

        public ref readonly T? ValueRef => ref _value;

        public bool IsEnd => _end;

        public NodeChildrenCollection Children => new(this);

        internal ReadOnlySpan<Node> ChildrenSpan => _children.AsSpan(0, _childCount);

        internal int ChildCount => _childCount;

        internal void AddChildAndSetParent(Node child)
        {
            Debug.Assert(child._parent is null);

            if (_childCount == _children.Length) {
                ArrayGrowHelper.Grow(ref _children, _childCount + 1, _childCount);
            }

            child._parent = this;
            _children[_childCount++] = child;
        }

        internal void RemoveChild(Node child)
        {
            Debug.Assert(child._parent == this);

            int i = ChildIndexOf(child);
            Debug.Assert(i >= 0);
            ArrayGrowHelper.ShiftLeftForRemoveAndFree(_children, _childCount, i, 1);
            _childCount--;
        }

        internal void ClearChildren()
        {
            Array.Clear(_children, 0, _childCount);
            _childCount = 0;
        }

        private int ChildIndexOf(Node child)
        {
            for (int i = 0; i < _childCount; i++) {
                if (_children[i] == child)
                    return i;
            }
            return -1;
        }
    }

    public readonly struct NodeChildrenCollection : IEnumerable<Node>
    {
        private readonly Node _node;

        public bool IsEmpty => _node.ChildCount == 0;

        internal NodeChildrenCollection(Node node)
            => _node = node;

        public Enumerator GetEnumerator() => new(_node);
        readonly IEnumerator<Node> IEnumerable<Node>.GetEnumerator() => GetEnumerator();
        readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public struct Enumerator : IEnumerator<Node>
        {
            private readonly Node _parent;
            private readonly int _version;
            private int _index;
            private Node? _current;

            internal Enumerator(Node node)
            {
                _parent = node;
                _version = node._tree._version;
                _index = 0;
            }

            public readonly Node Current => _current!;

            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                if (_version != _parent._tree._version)
                    Throws.CollectionModifiedDuringEnumeration();

                var children = _parent.ChildrenSpan;
                if (_index >= children.Length)
                    return false;

                _current = children[_index];
                _index++;
                return true;
            }

            void IEnumerator.Reset() => _index = 0;
            readonly void IDisposable.Dispose() { }
        }
    }
}
