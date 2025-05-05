using CommunityToolkit.Diagnostics;
using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Trarizon.Library.Collections.Helpers;

namespace Trarizon.Library.Collections.Generic;
public class PrefixTreeDictionary<TKey, TValue> where TKey : notnull
{
    private Node? _root;
    private int _count;
    private int _nodeCount;
    private int _version;
    private IEqualityComparer<TKey>? _comparer;

    public PrefixTreeDictionary()
    {
    }

    public PrefixTreeDictionary(IEqualityComparer<TKey>? comparer)
    {
        _comparer = comparer;
    }

    #region Access

    public int Count => _count;

    public int NodeCount => _nodeCount;

    public Node Root => GetEnsuredRoot();

    public bool TryGetValue(ReadOnlySpan<TKey> sequence, [MaybeNullWhen(false)] out TValue value)
    {
        var node = FindPrefix(sequence);
        if (node is { IsEnd: true }) {
            value = node.GetValueOrDefault()!;
            return true;
        }
        else {
            value = default;
            return false;
        }
    }

    public bool TryGetValue(IEnumerable<TKey> sequence, [MaybeNullWhen(false)] out TValue value)
    {
        var node = FindPrefix(sequence);
        if (node is { IsEnd: true }) {
            value = node.GetValueOrDefault()!;
            return true;
        }
        else {
            value = default;
            return false;
        }
    }

    public bool ContainsKey(ReadOnlySpan<TKey> sequence)
    {
        var node = FindPrefix(sequence);
        if (node is null)
            return false;
        return node.IsEnd;
    }

    public bool ContainsKey(IEnumerable<TKey> sequence)
    {
        var node = FindPrefix(sequence);
        if (node is null)
            return false;
        return node.IsEnd;
    }

    public bool ContainsKeyPrefix(ReadOnlySpan<TKey> sequence)
    {
        var node = FindPrefix(sequence);
        if (node is null)
            return false;
        return true;
    }

    public bool ContainsKeyPrefix(IEnumerable<TKey> sequence)
    {
        var node = FindPrefix(sequence);
        if (node is null)
            return false;
        return true;
    }

    private Node? FindPrefix(ReadOnlySpan<TKey> prefix)
    {
        if (_root is null)
            return null;

        var node = _root;

        if (typeof(TKey).IsValueType && _comparer is null) {
            foreach (var val in prefix) {
                var child = node._child;
                while (child is not null) {
                    if (EqualityComparer<TKey>.Default.Equals(child.Key!, val)) {
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
            var comparer = _comparer ??= EqualityComparer<TKey>.Default;
            foreach (var val in prefix) {
                var child = node._child;
                while (child is not null) {
                    if (comparer.Equals(child.Key!, val)) {
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

    private Node? FindPrefix(IEnumerable<TKey> prefix)
    {
        if (_root is null)
            return null;

        var node = _root;

        if (typeof(TKey).IsValueType && _comparer is null) {
            foreach (var val in prefix) {
                var child = node._child;
                while (child is not null) {
                    if (EqualityComparer<TKey>.Default.Equals(child.Key!, val)) {
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
            var comparer = _comparer ??= EqualityComparer<TKey>.Default;
            foreach (var val in prefix) {
                var child = node._child;
                while (child is not null) {
                    if (comparer.Equals(child.Key!, val)) {
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

    public Node GetOrAdd(ReadOnlySpan<TKey> sequence, TValue value)
    {
        var node = GetEnsuredRoot();
        foreach (var item in sequence) {
            node = GetOrAddChild(node, item, value);
        }
        _version++;
        _count++;

        node.IsEnd = true;
        return node;
    }

    public Node GetOrAdd(IEnumerable<TKey> sequence, TValue value)
    {
        var node = GetEnsuredRoot();
        foreach (var item in sequence) {
            node = GetOrAddChild(node, item, value);
        }
        _version++;
        _count++;

        node.IsEnd = true;
        return node;
    }

    public bool TryAdd(ReadOnlySpan<TKey> sequence, TValue value, out Node node)
    {
        node = GetEnsuredRoot();
        foreach (var item in sequence) {
            node = GetOrAddChild(node, item, value);
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

    public bool TryAdd(IEnumerable<TKey> sequence, TValue value, out Node node)
    {
        node = GetEnsuredRoot();
        foreach (var item in sequence) {
            node = GetOrAddChild(node, item, value);
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

    public bool Remove(ReadOnlySpan<TKey> sequence)
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

    public bool Remove(IEnumerable<TKey> sequence)
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

    private Node GetOrAddChild(Node parent, TKey key, TValue value)
    {
        var child = parent._child;
        if (typeof(TKey).IsValueType && _comparer is null) {
            while (child is not null) {
                if (EqualityComparer<TKey>.Default.Equals(key, child.Key!))
                    return child;
                child = child._nextSibling;
            }
        }
        else {
            var comparer = _comparer ??= EqualityComparer<TKey>.Default;
            while (child is not null) {
                if (comparer.Equals(key, child.Key!))
                    return child;
                child = child._nextSibling;
            }
        }

        {
            child = new Node(this, key, value);
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
        internal PrefixTreeDictionary<TKey, TValue> _tree;
        internal Node? _parent;
        internal Node? _child;
        internal Node? _nextSibling;
        private readonly TKey _key;
        private TValue? _value;

        internal Node(PrefixTreeDictionary<TKey, TValue> tree, TKey key, TValue? value = default)
        {
            _tree = tree;
            _key = key;
        }

        [MemberNotNullWhen(true, nameof(_value))]
        public bool IsEnd { get; internal set; }

        public TKey? Key => _key;

        public ref readonly TKey KeyRef => ref _key;

        public TValue Value
        {
            get {
                if (!IsEnd)
                    ThrowHelper.ThrowInvalidOperationException("The node doesn't contains a value");
                return _value!;
            }
            set {
                if (!IsEnd)
                    ThrowHelper.ThrowInvalidOperationException("The node doesn't contains a value");
                _value = value;
            }
        }

        /// <summary>
        /// Value ref, if the node doesnt contains a value, return a null ref
        /// </summary>
        public ref TValue ValueRef
        {
            get {
                if (!IsEnd) {
                    return ref Unsafe.NullRef<TValue>();
                }
                else {
                    return ref _value!;
                }
            }
        }

        public NodeChildrenCollection Children => new(this);

        public TValue? GetValueOrDefault() => _value;
    }

    public readonly struct NodeChildrenCollection : IEnumerable<Node>
    {
        private readonly Node _node;

        public bool IsEmpty => _node._child is null;

        internal NodeChildrenCollection(Node node)
        {
            _node = node;
        }

        public readonly Enumerator GetEnumerator() => new Enumerator(_node);
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
