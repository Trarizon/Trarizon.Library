using CommunityToolkit.Diagnostics;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library.Collections.Generic;

#if CONTIGUOUS_COLLECTIONS

partial class ContiguousLinkedList<T>
{
    #region Access

    public Node GetNode(ContiguousLinkedListNodeIndex nodeIndex)
    {
        if (!TryGetNode(nodeIndex, out var node))
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(nodeIndex));
        return node;
    }

    public bool TryGetNode(ContiguousLinkedListNodeIndex nodeIndex, out Node node)
    {
        var index = nodeIndex.Value;
        if (index < 0 || index >= _entries.Length) {
            node = default;
            return false;
        }
        ref var entry = ref _entries[index];
        if (entry.Version != nodeIndex._version) {
            node = default;
            return false;
        }
        // NodeIndex wont own a freed node
        Debug.Assert(!entry.IsFreed);
        node = new Node(this, index);
        return true;
    }

    #endregion

    #region Modification

    public Node AddBefore(ContiguousLinkedListNodeIndex index, T value)
    {
        var node = GetNode(index);
        return AddBefore(node, value);
    }

    public Node AddAfter(ContiguousLinkedListNodeIndex index, T value)
    {
        var node = GetNode(index);
        return AddAfter(node, value);
    }

    public bool RemoveAt(ContiguousLinkedListNodeIndex nodeIndex)
    {
        if (TryGetNode(nodeIndex, out var node)) {
            var ret = Remove(node);
            Debug.Assert(ret is true);
            return true;
        }
        return false;
    }

    #endregion

    partial struct Node
    {
        public ContiguousLinkedListNodeIndex GetIndex()
        {
            if (!HasValue)
                return default;
            ref readonly var entry = ref DangerousGetEntryRef();
            if (entry.Version != _version)
                return default;
            Debug.Assert(!entry.IsFreed);
            return new ContiguousLinkedListNodeIndex(_index, _version);
        }
    }
}

/// <remarks>
/// <see cref="Node"/>s get item by index, If a node is removed, the associated <see cref="Node"/> or <see cref="ContiguousLinkedListNodeIndex"/> should immediately
/// set to invalid, or else unexcepted data may be get.
/// </remarks>
public readonly struct ContiguousLinkedListNodeIndex : IEquatable<ContiguousLinkedListNodeIndex>
{
    // To avoid getting a valid value through default(T), we treat 0 as invalid
    internal readonly int _value;
    internal readonly int _version;

    public static ContiguousLinkedListNodeIndex Null => default;

    internal ContiguousLinkedListNodeIndex(int entryIndex, int version)
    {
        Debug.Assert(entryIndex >= 0);
        _value = entryIndex + 1;
        _version = version;
    }

    internal int Value => _value - 1;

    public bool Equals(ContiguousLinkedListNodeIndex other) => _value == other._value && _version == other._version;
    public override int GetHashCode() => HashCode.Combine(_value, _version);
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is ContiguousLinkedListNodeIndex o && Equals(o);
    public static bool operator ==(ContiguousLinkedListNodeIndex left, ContiguousLinkedListNodeIndex right) => left.Equals(right);
    public static bool operator !=(ContiguousLinkedListNodeIndex left, ContiguousLinkedListNodeIndex right) => !left.Equals(right);
}

#endif
