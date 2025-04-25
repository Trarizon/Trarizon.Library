using CommunityToolkit.Diagnostics;
using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library.Collections.Generic;
public partial class ContiguousLinkedList<T>
{
#if DEBUG
    public IEnumerable<(bool IsActive, T Value)> EnumerateEntries()
    {
        foreach (var entry in _entries) {
            yield return (!entry.IsFreed, entry.Value);
        }
    }
#endif

    public UnsafeNode GetNode(ContiguousLinkedListUnsafeNodeIndex nodeIndex)
    {
        if (!TryGetNode(nodeIndex, out var node))
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(nodeIndex));
        return node;
    }

    public bool TryGetNode(ContiguousLinkedListUnsafeNodeIndex nodeIndex, out UnsafeNode node)
    {
        var index = nodeIndex.Value;
        if (index < 0 || index >= _entries.Length) {
            node = default;
            return false;
        }
        ref var entry = ref _entries[index];
        if (entry.IsFreed) {
            node = default;
            return false;
        }
        node = new Node(this, index);
        return true;
    }

    public UnsafeNode AddAfter(UnsafeNode node, T value)
    {
        ValidateNodeBelonging(node);
        return AddAfterInternal(node._index, value);
    }

    public UnsafeNode AddBefore(UnsafeNode node, T value)
    {
        ValidateNodeBelonging(node);
        return AddBeforeInternal(node._index, value);
    }

    public UnsafeNode AddBefore(ContiguousLinkedListUnsafeNodeIndex index, T value)
    {
        _ = GetNode(index);
        return AddBeforeInternal(index.Value, value);
    }

    public UnsafeNode AddAfter(ContiguousLinkedListUnsafeNodeIndex index, T value)
    {
        _ = GetNode(index);
        return AddAfterInternal(index.Value, value);
    }

    public bool Remove(UnsafeNode node)
    {
        ValidateNodeBelonging(node);
        if (node.HasValue) {
            RemoveEntry(node._index, ref node.Entry);
            return true;
        }
        return false;
    }

    public bool RemoveAt(ContiguousLinkedListUnsafeNodeIndex nodeIndex)
    {
        if (TryGetNode(nodeIndex, out var node)) {
            RemoveEntry(node._index, ref node.Entry);
            return true;
        }
        return false;
    }

    public UnsafeNodesIterator EnumerateUnsafeNodes() => new(this);

    /// <summary>
    /// This node does not ensure the value is valid and expected, you may get different data
    /// if you remove and then add some items to the list
    /// </summary>
    public readonly struct UnsafeNode : IEquatable<UnsafeNode>
    {
        internal readonly ContiguousLinkedList<T> _list;
        internal readonly int _index;

        public bool HasValue => _list is not null;

        public T Value
        {
            get => Entry.Value;
            set {
                Entry.Value = value;
                _list._version++;
            }
        }

        public UnsafeNode Next
        {
            get {
                if (!HasValue)
                    return default;
                ref readonly var entry = ref Entry;
                if (entry.IsFreed)
                    return default;
                if (entry.Next == _list._firstIndex)
                    return default;
                return new(_list, entry.Next);
            }
        }

        public UnsafeNode Prev
        {
            get {
                if (!HasValue)
                    return default;
                ref readonly var entry = ref Entry;
                if (entry.IsFreed)
                    return default;
                if (_index == _list._firstIndex)
                    return default;
                return new UnsafeNode(_list, entry.Prev);
            }
        }

        internal ref Entry Entry => ref _list._entries[_index];

        internal UnsafeNode(ContiguousLinkedList<T> list, int index)
        {
            _list = list;
            _index = index;
        }

        public ContiguousLinkedListUnsafeNodeIndex GetIndex()
        {
            if (!HasValue)
                return default;
            ref readonly var entry = ref Entry;
            if (entry.IsFreed)
                return default;
            return new ContiguousLinkedListUnsafeNodeIndex(_index);
        }

        public ref T GetValueRef() => ref Entry.Value;

        public Node ToNode() => new(_list, _index);

        public static implicit operator UnsafeNode(Node node)
            => new(node._list, node._index);

        public bool Equals(UnsafeNode other) => _list == other._list && _index == other._index;
        public override int GetHashCode() => HashCode.Combine(_list, _index);
        public override bool Equals([NotNullWhen(true)] object? obj) => obj is UnsafeNode other && Equals(other);
        public static bool operator ==(UnsafeNode left, UnsafeNode right) => left.Equals(right);
        public static bool operator !=(UnsafeNode left, UnsafeNode right) => !left.Equals(right);
    }

    public struct UnsafeNodesIterator : IEnumerable<UnsafeNode>, IEnumerator<UnsafeNode>
    {
        private readonly ContiguousLinkedList<T> _list;
        private int _index;
        private readonly int _version;
        private int _current;

        public readonly UnsafeNode Current => new(_list, _current);

        readonly object IEnumerator.Current => Current;

        internal UnsafeNodesIterator(ContiguousLinkedList<T> list)
        {
            _list = list;
            _index = _list._firstIndex;
            _version = _list._version;
        }

        public readonly UnsafeNodesIterator GetEnumerator() => new UnsafeNodesIterator(_list);

        public bool MoveNext()
        {
            ValidateVersion();

            if (_index < 0)
                return false;

            _current = _index;
            _index = _list._entries[_index].Next;
            if (_index == _list._firstIndex)
                _index = -1;
            return true;
        }

        private readonly void ValidateVersion()
        {
            if (_list._version != _version)
                TraThrow.CollectionModified();
        }

        public void Reset() => _index = _list._firstIndex;
        readonly void IDisposable.Dispose() { }

        readonly IEnumerator<UnsafeNode> IEnumerable<UnsafeNode>.GetEnumerator() => GetEnumerator();
        readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

/// <summary>
/// This index does not ensure the value is valid and expected, you may get different data
/// if you remove and then add some items to the list
/// </summary>
public readonly struct ContiguousLinkedListUnsafeNodeIndex : IEquatable<ContiguousLinkedListUnsafeNodeIndex>
{
    private readonly int _value;

    public static ContiguousLinkedListUnsafeNodeIndex Null => default;

    internal int Value => _value - 1;

    internal ContiguousLinkedListUnsafeNodeIndex(int entryIndex)
    {
        Debug.Assert(entryIndex >= 0);
        _value = entryIndex + 1;
    }

    public ContiguousLinkedListNodeIndex ToNodeIndex<T>(ContiguousLinkedList<T> ofList)
    {
        if (_value == 0)
            return ContiguousLinkedListNodeIndex.Null;
        var version = ofList._entries[Value].Version;
        return new ContiguousLinkedListNodeIndex(Value, version);
    }

    public static implicit operator ContiguousLinkedListUnsafeNodeIndex(ContiguousLinkedListNodeIndex index)
        => new(index._value);

    public bool Equals(ContiguousLinkedListUnsafeNodeIndex other) => _value == other._value;
    public override int GetHashCode() => _value.GetHashCode();
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is ContiguousLinkedListUnsafeNodeIndex o && Equals(o);
    public static bool operator ==(ContiguousLinkedListUnsafeNodeIndex left, ContiguousLinkedListUnsafeNodeIndex right) => left.Equals(right);
    public static bool operator !=(ContiguousLinkedListUnsafeNodeIndex left, ContiguousLinkedListUnsafeNodeIndex right) => !left.Equals(right);
}
