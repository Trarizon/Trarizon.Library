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
    private readonly IEqualityComparer<TKey>? _comparer;

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

    public IEqualityComparer<TKey> Comparer => _comparer ?? EqualityComparer<TKey>.Default;

    public TValue this[ReadOnlySpan<TKey> sequence]
    {
        get {
            if (!TryGetValue(sequence, out var value))
                Throws.KeyNotFound(sequence, nameof(PrefixTreeDictionary<,>));
            return value;
        }
        set {
            if (!TryAdd(sequence, value, out var node)) {
                node.SetIsEnd(value);
                _version++;
            }
        }
    }

    public PrefixTreeDictionary(IEqualityComparer<TKey>? comparer = null)
    {
        if (!typeof(TKey).IsValueType)
            _comparer = comparer ?? EqualityComparer<TKey>.Default;
        else if (comparer is not null && !ReferenceEquals(comparer, EqualityComparer<TKey>.Default))
            _comparer = comparer;
        _nodeCount = 1;
    }

    public bool TryGetValue(ReadOnlySpan<TKey> sequence, [MaybeNullWhen(false)] out TValue value)
    {
        var node = FindPrefixNode(sequence);
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
        var node = FindPrefixNode(sequence);
        if (node is { IsEnd: true }) {
            value = node.GetValueOrDefault()!;
            return true;
        }
        else {
            value = default;
            return false;
        }
    }

    public bool TryGetNode(ReadOnlySpan<TKey> prefixSequence, [MaybeNullWhen(false)] out Node node)
    {
        node = FindPrefixNode(prefixSequence);
        return node is not null;
    }

    public bool TryGetNode(IEnumerable<TKey> prefixSequence, [MaybeNullWhen(false)] out Node node)
    {
        node = FindPrefixNode(prefixSequence);
        return node is not null;
    }

    public bool ContainsKey(ReadOnlySpan<TKey> sequence)
        => FindPrefixNode(sequence) is { IsEnd: true };

    public bool ContainsKey(IEnumerable<TKey> sequence)
        => FindPrefixNode(sequence) is { IsEnd: true };

    public bool ContainsKeyPrefix(ReadOnlySpan<TKey> prefixSequence)
        => FindPrefixNode(prefixSequence) is not null;

    public bool ContainsKeyPrefix(IEnumerable<TKey> prefixSequence)
        => FindPrefixNode(prefixSequence) is not null;

    private Node? FindPrefixNode(ReadOnlySpan<TKey> prefix)
    {
        if (prefix.IsEmpty)
            return Root;

        if (_root is null)
            return null;

        var node = _root;

        if (typeof(TKey).IsValueType && _comparer is null) {
            foreach (var val in prefix) {
                var children = node.ChildrenSpan;
                foreach (var child in children) {
                    if (EqualityComparer<TKey>.Default.Equals(child.Key!, val)) {
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
                    if (comparer.Equals(child.Key!, val)) {
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

    private Node? FindPrefixNode(IEnumerable<TKey> prefix)
    {
        if (_root is null)
            return null;

        var node = _root;

        if (typeof(TKey).IsValueType && _comparer is null) {
            foreach (var val in prefix) {
                var children = node.ChildrenSpan;
                foreach (var child in children) {
                    if (EqualityComparer<TKey>.Default.Equals(child.Key!, val)) {
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
                    if (comparer.Equals(child.Key!, val)) {
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

    public Node GetOrAdd(ReadOnlySpan<TKey> sequence, TValue value)
    {
        TryAdd(sequence, value, out var node);
        return node;
    }

    public Node GetOrAdd(IEnumerable<TKey> sequence, TValue value)
    {
        TryAdd(sequence, value, out var node);
        return node;
    }

    public bool TryAdd(ReadOnlySpan<TKey> sequence, TValue value, out Node node)
    {
        node = Root;
        foreach (var item in sequence) {
            node = GetOrAddChild(node, item);
        }

        if (node.IsEnd) {
            return false;
        }

        _version++;
        _count++;
        node.SetIsEnd(value);
        return true;
    }

    public bool TryAdd(IEnumerable<TKey> sequence, TValue value, out Node node)
    {
        node = Root;
        foreach (var item in sequence) {
            node = GetOrAddChild(node, item);
        }

        if (node.IsEnd) {
            return false;
        }
        _version++;
        _count++;
        node.SetIsEnd(value);
        return true;
    }

    private Node GetOrAddChild(Node parent, TKey key)
    {
        Debug.Assert(parent._tree == this);

        if (typeof(TKey).IsValueType && _comparer is null) {
            foreach (var child in parent.ChildrenSpan) {
                if (EqualityComparer<TKey>.Default.Equals(child.Key!, key)) {
                    return child;
                }
            }
        }
        else {
            Debug.Assert(_comparer is not null);
            var comparer = _comparer!;
            foreach (var child in parent.ChildrenSpan) {
                if (comparer.Equals(child.Key!, key)) {
                    return child;
                }
            }
        }

        var node = new Node(this, key);
        parent.AddChildAndSetParent(node);
        _nodeCount++;
        return node;
    }

    public bool Remove(ReadOnlySpan<TKey> sequence)
    {
        var node = FindPrefixNode(sequence);
        if (node is not { IsEnd: true })
            return false;

        RemoveEndNode(node);
        _count--;
        _version++;
        return true;
    }

    public bool Remove(IEnumerable<TKey> sequence)
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
        Debug.Assert(endNode._tree == this && endNode.IsEnd);
        Debug.Assert(_root is not null, "If there's any node in the tree, the root node should have been initialized");

        endNode.SetNotEnd();
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
        internal PrefixTreeDictionary<TKey, TValue> _tree;
        internal Node? _parent;
        private Node[] _children;
        private int _childCount = 0;
        private readonly TKey? _key;
        private TValue? _value;
        private bool _end = false;

        internal Node(PrefixTreeDictionary<TKey, TValue> tree, TKey? key)
        {
            _tree = tree;
            _children = [];
            _key = key;
        }

        public Node? Parent => _parent;

        public TKey? Key => _key;

        public ref readonly TKey? KeyRef => ref _key;

        public TValue Value
        {
            get {
                if (!IsEnd) ThrowNoValue();
                return _value!;
            }
            set {
                if (!IsEnd) ThrowNoValue();
                _value = value;
            }
        }

        public ref TValue ValueRef
        {
            get {
                if (!IsEnd) ThrowNoValue();
                return ref _value!;
            }
        }

        [MemberNotNullWhen(true, nameof(_value))]
        public bool IsEnd => _end;

        public NodeChildrenCollection Children => new(this);

        internal ReadOnlySpan<Node> ChildrenSpan => _children.AsSpan(0, _childCount);

        public TValue? GetValueOrDefault() => _value;

        public ref TValue GetValueRefOrNullRef() => ref IsEnd ? ref _value! : ref Unsafe.NullRef<TValue>();

        internal void SetIsEnd(TValue value)
        {
            _value = value;
            _end = true;
        }

        internal void SetNotEnd()
        {
            _end = false;
            _value = default;
        }

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

        [DoesNotReturn]
        private static void ThrowNoValue()
            => Throws.ThrowInvalidOperation("The node doesn't contains a value");
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
