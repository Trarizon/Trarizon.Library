using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Trarizon.Library.Collections.Helpers;

namespace Trarizon.Library.Collections.Generic;
public partial class PrefixTreeDictionary<TKey, TValue> where TKey : notnull
{
    internal Node? _root;
    internal int _count;
    private int _nodeCount;
    internal int _version;
    private IEqualityComparer<TKey>? _comparer;

    public PrefixTreeDictionary() : this(null)
    {
    }

    public PrefixTreeDictionary(IEqualityComparer<TKey>? comparer)
    {
        if (!typeof(TKey).IsValueType) {
            _comparer = comparer ?? EqualityComparer<TKey>.Default;
        }
        else if (comparer is not null && !ReferenceEquals(comparer, EqualityComparer<TKey>.Default)) {
            _comparer = comparer;
        }
    }

    #region Access

    public int Count => _count;

    public int NodeCount => _nodeCount;

    /// <summary>
    /// Root's <see cref="Node.Key"/> is always default
    /// </summary>
    public Node Root => GetEnsuredRoot();

    public IEqualityComparer<TKey> Comparer => _comparer ?? EqualityComparer<TKey>.Default;

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

    public bool TryGetNode(ReadOnlySpan<TKey> sequence, [MaybeNullWhen(false)] out Node node)
    {
        node = FindPrefix(sequence);
        return node is not null;
    }

    public bool TryGetNode(IEnumerable<TKey> sequence, [MaybeNullWhen(false)] out Node node)
    {
        node = FindPrefix(sequence);
        return node is not null;
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
            Debug.Assert(_comparer is not null);
            var comparer = _comparer!;
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
            Debug.Assert(_comparer is not null);
            var comparer = _comparer!;
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
        => GetOrAddInternal(sequence, value, out _);

    public Node GetOrAdd(IEnumerable<TKey> sequence, TValue value)
        => GetOrAddInternal(sequence, value, out _);

    public bool TryAdd(ReadOnlySpan<TKey> sequence, TValue value, [MaybeNullWhen(false)] out Node node)
    {
        var n = GetOrAddInternal(sequence, value, out var exists);
        if (exists) {
            node = default;
            return false;
        }
        else {
            node = n;
            return true;
        }
    }

    public bool TryAdd(IEnumerable<TKey> sequence, TValue value, [MaybeNullWhen(false)] out Node node)
    {
        var n = GetOrAddInternal(sequence, value, out var exists);
        if (exists) {
            node = default;
            return false;
        }
        else {
            node = n;
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

        RemoveInternal(node);
        return true;
    }

    public bool Remove(IEnumerable<TKey> sequence)
    {
        var node = FindPrefix(sequence);
        if (node is null)
            return false;
        if (!node.IsEnd)
            return false;

        RemoveInternal(node);
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

    private Node GetOrAddInternal(ReadOnlySpan<TKey> sequence, TValue value, out bool exists)
    {
        var node = GetEnsuredRoot();
        foreach (var item in sequence) {
            node = GetOrAddChild(node, item);
        }

        _version++;
        _count++;

        if (node.IsEnd) {
            exists = true;
        }
        else {
            exists = false;
            node.SetIsEnd(value);
        }
        return node;
    }

    private Node GetOrAddInternal(IEnumerable<TKey> sequence, TValue value, out bool exists)
    {
        var node = GetEnsuredRoot();
        foreach (var item in sequence) {
            node = GetOrAddChild(node, item);
        }

        _version++;
        _count++;

        if (node.IsEnd) {
            exists = true;
        }
        else {
            exists = false;
            node.SetIsEnd(value);
        }
        return node;
    }

    private Node GetOrAddChild(Node parent, TKey key)
    {
        Debug.Assert(parent._tree == this);

        var child = parent._child;
        if (typeof(TKey).IsValueType && _comparer is null) {
            while (child is not null) {
                if (EqualityComparer<TKey>.Default.Equals(key, child.Key!))
                    return child;
                child = child._nextSibling;
            }
        }
        else {
            Debug.Assert(_comparer is not null);
            var comparer = _comparer!;
            while (child is not null) {
                if (comparer.Equals(key, child.Key!))
                    return child;
                child = child._nextSibling;
            }
        }

        {
            child = new Node(this, key);
            child._parent = parent;
            child._nextSibling = parent._child;
            parent._child = child;
            _nodeCount++;
            return child;
        }
    }

    internal void RemoveInternal(Node endNode)
    {
        Debug.Assert(endNode._tree == this && endNode.IsEnd);

        _version++;
        _count--;
        endNode.SetNotEnd();
        if (endNode._child is null) {
            InvalidateFromLeaf(endNode);
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

        internal Node(PrefixTreeDictionary<TKey, TValue> tree, TKey key)
        {
            _tree = tree;
            _key = key;
        }

        [MemberNotNullWhen(true, nameof(_value))]
        public bool IsEnd { get; private set; }

        /// <summary>
        /// If the node is root, the key is always default
        /// </summary>
        public TKey Key => _key;

        public ref readonly TKey KeyRef => ref _key;

        public TValue Value
        {
            get {
                if (!IsEnd)
                    ThrowNoValue();
                return _value!;
            }
            set {
                if (!IsEnd)
                    ThrowNoValue();
                _value = value;
            }
        }

        /// <summary>
        /// Value ref
        /// </summary>
        public ref TValue ValueRef
        {
            get {
                if (!IsEnd)
                    ThrowNoValue();
                return ref _value!;
            }
        }

        public Node? Parent => _parent;

        public NodeChildrenCollection Children => new(this);

        public TValue? GetValueOrDefault() => _value;

        public ref TValue GetValueRefOrNullRef() => ref IsEnd ? ref _value! : ref Unsafe.NullRef<TValue>();

        internal void SetIsEnd(TValue value)
        {
            _value = value;
            IsEnd = true;
        }

        internal void SetNotEnd()
        {
            IsEnd = false;
            _value = default;
        }

        [DoesNotReturn]
        private static void ThrowNoValue()
            => Throws.ThrowInvalidOperation("The node doesn't contains a value");
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
                    Throws.CollectionModifiedDuringEnumeration();

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
