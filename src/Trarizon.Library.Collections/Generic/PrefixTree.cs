using System.Collections;
using System.Diagnostics;
using Trarizon.Library.Collections.Helpers;

namespace Trarizon.Library.Collections.Generic;
public class PrefixTree<T>
{
    private Node? _root;
    private int _count;
    private int _nodeCount;
    private int _version;
    private IEqualityComparer<T>? _comparer;

    public PrefixTree()
    {
    }

    public PrefixTree(IEqualityComparer<T>? comparer)
    {
        _comparer = comparer;
    }

    #region Access

    public int Count => _count;

    public int NodeCount => _nodeCount;

    /// <summary>
    /// Root's <see cref="Node.Value"/> is always default
    /// </summary>
    public Node Root => GetEnsuredRoot();

    public bool Contains(ReadOnlySpan<T> sequence)
    {
        var node = FindPrefix(sequence);
        if (node is null)
            return false;
        return node.IsEnd;
    }

    public bool Contains(IEnumerable<T> sequence)
    {
        var node = FindPrefix(sequence);
        if (node is null)
            return false;
        return node.IsEnd;
    }

    public bool ContainsPrefix(ReadOnlySpan<T> sequence)
    {
        var node = FindPrefix(sequence);
        if (node is null)
            return false;
        return true;
    }

    public bool ContainsPrefix(IEnumerable<T> sequence)
    {
        var node = FindPrefix(sequence);
        if (node is null)
            return false;
        return true;
    }

    private Node? FindPrefix(ReadOnlySpan<T> prefix)
    {
        if (_root is null)
            return null;

        var node = _root;

        if (typeof(T).IsValueType && _comparer is null) {
            foreach (var val in prefix) {
                var child = node._child;
                while (child is not null) {
                    if (EqualityComparer<T>.Default.Equals(child.Value!, val)) {
                        node = child;
                        goto ContinueFor;
                    }
                    child = child._nextSibling;
                }
                return null;

            ContinueFor:
                continue;
            }
        }
        else {
            var comparer = _comparer ??= EqualityComparer<T>.Default;
            foreach (var val in prefix) {
                var child = node._child;
                while (child is not null) {
                    if (comparer.Equals(child.Value!, val)) {
                        node = child;
                        goto ContinueFor;
                    }
                    child = child._nextSibling;
                }
                return null;

            ContinueFor:
                continue;
            }
        }
        return node;
    }

    private Node? FindPrefix(IEnumerable<T> prefix)
    {
        if (_root is null)
            return null;

        var node = _root;

        if (typeof(T).IsValueType && _comparer is null) {
            foreach (var val in prefix) {
                var child = node._child;
                while (child is not null) {
                    if (EqualityComparer<T>.Default.Equals(child.Value!, val)) {
                        node = child;
                        goto ContinueFor;
                    }
                    child = child._nextSibling;
                }
                return null;

            ContinueFor:
                continue;
            }
        }
        else {
            var comparer = _comparer ??= EqualityComparer<T>.Default;
            foreach (var val in prefix) {
                var child = node._child;
                while (child is not null) {
                    if (comparer.Equals(child.Value!, val)) {
                        node = child;
                        goto ContinueFor;
                    }
                    child = child._nextSibling;
                }
                return null;

            ContinueFor:
                continue;
            }
        }
        return node;
    }

    #endregion

    #region Modification

    public Node GetOrAdd(ReadOnlySpan<T> sequence)
    {
        var node = GetEnsuredRoot();
        foreach (var item in sequence) {
            node = GetOrAddChild(node, item);
        }
        _version++;
        _count++;

        node.IsEnd = true;
        return node;
    }

    public Node GetOrAdd(IEnumerable<T> sequence)
    {
        var node = GetEnsuredRoot();
        foreach (var item in sequence) {
            node = GetOrAddChild(node, item);
        }
        _version++;
        _count++;

        node.IsEnd = true;
        return node;
    }

    public bool TryAdd(ReadOnlySpan<T> sequence, out Node node)
    {
        node = GetEnsuredRoot();
        foreach (var item in sequence) {
            node = GetOrAddChild(node, item);
        }

        _version++;
        _count++;

        if (node.IsEnd) {
            return false;
        }
        else {
            node.IsEnd = true;
            return true;
        }
    }

    public bool TryAdd(IEnumerable<T> sequence, out Node node)
    {
        node = GetEnsuredRoot();
        foreach (var item in sequence) {
            node = GetOrAddChild(node, item);
        }

        _version++;
        _count++;

        if (node.IsEnd) {
            return false;
        }
        else {
            node.IsEnd = true;
            return true;
        }
    }

    public bool Remove(ReadOnlySpan<T> sequence)
    {
        var node = FindPrefix(sequence);
        if (node is null)
            return false;
        if (!node.IsEnd)
            return false;

        _version++;
        _count--;
        node.IsEnd = false;
        if (node._child is null) {
            InvalidateFromLeaf(node);
        }
        return true;
    }

    public bool Remove(IEnumerable<T> sequence)
    {
        var node = FindPrefix(sequence);
        if (node is null)
            return false;
        if (!node.IsEnd)
            return false;

        _version++;
        _count--;
        node.IsEnd = false;
        if (node._child is null) {
            InvalidateFromLeaf(node);
        }
        return true;
    }

    public void Clear()
    {
        _count = 0;
        _nodeCount = 0;
        if (_root is not null) {
            _root._child = null;
        }
        _version++;
    }

    private Node GetOrAddChild(Node parent, T value)
    {
        var child = parent._child;
        if (typeof(T).IsValueType && _comparer is null) {
            while (child is not null) {
                if (EqualityComparer<T>.Default.Equals(value, child.Value!))
                    return child;
                child = child._nextSibling;
            }
        }
        else {
            var comparer = _comparer ??= EqualityComparer<T>.Default;
            while (child is not null) {
                if (comparer.Equals(value, child.Value!))
                    return child;
                child = child._nextSibling;
            }
        }

        {
            child = new Node(this, value);
            child._parent = parent;
            child._nextSibling = parent._child;
            parent._child = child;
            _nodeCount++;
            return child;
        }
    }

    private void InvalidateFromLeaf(Node node)
    {
        Debug.Assert(node._child is null);

        do {
            Debug.Assert(node._parent is not null);
            var parent = node._parent!;
            Debug.Assert(parent._child is not null);
            var child = parent._child!;

            // node is not the first child, remove it from children list
            if (child != node) {
                while (child._nextSibling != node) {
                    Debug.Assert(child._nextSibling is not null);
                    child = child._nextSibling!;
                }
                child._nextSibling = node._nextSibling;
            }
            _nodeCount--;
            node = parent;
        } while (node != _root && !node.IsEnd);
    }

    #endregion

    private Node GetEnsuredRoot() => _root ??= new Node(this, default!);

    public sealed class Node
    {
        internal PrefixTree<T> _tree;
        internal Node? _parent;
        internal Node? _child;
        internal Node? _nextSibling;
        private readonly T _value;

        internal Node(PrefixTree<T> tree, T value)
        {
            _tree = tree;
            _value = value;
        }

        public bool IsEnd { get; internal set; }

        /// <summary>
        /// If the node is root, the value is always default
        /// </summary>
        public T Value => _value;

        public ref readonly T ValueRef => ref _value;

        public NodeChildrenCollection Children => new(this);
    }

    public readonly struct NodeChildrenCollection : IEnumerable<Node>
    {
        private readonly Node _node;

        public bool IsEmpty => _node._child is null;

        internal NodeChildrenCollection(Node node)
        {
            _node = node;
        }

        public readonly Enumerator GetEnumerator() => new(_node);
        IEnumerator<Node> IEnumerable<Node>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public struct Enumerator : IEnumerator<Node>
        {
            private readonly Node _parent;
            private readonly int _version;
            private Node? _node;
            private Node? _current;

            public readonly Node Current => _current!;

            internal Enumerator(Node node)
            {
                _parent = node;
                _version = node._tree._version;
                _node = node._child;
            }

            public bool MoveNext()
            {
                if (_node is null)
                    return false;

                if (_version != _parent._tree._version)
                    Throws.CollectionModifiedAfterEnumeratorCreated();

                _current = _node;
                _node = _node._nextSibling;
                return true;
            }

            public void Reset() => _node = _parent._child;
            readonly object IEnumerator.Current => Current;
            readonly void IDisposable.Dispose() { }
        }
    }
}