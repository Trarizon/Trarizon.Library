using CommunityToolkit.Diagnostics;
using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Trarizon.Library.Numerics;

namespace Trarizon.Library.Collections.Generic;
/// <summary>
/// A linked list that store items in an array, also as a collection that won't move item index when removing item
/// </summary>
/// <typeparam name="T"></typeparam>
[CollectionBuilder(typeof(CollectionBuilders), nameof(CollectionBuilders.CreateContiguousLinkedList))]
public partial class ContiguousLinkedList<T> : ICollection<T>, IReadOnlyCollection<T>
{
    internal Entry[] _entries;
    private int _firstIndex;
    private int _count;
    // _count + freeList.Count
    private int _consumedCount;
    // Index of first item of free list, -1 if no free item
    private int _freeFirstIndex;
    private int _version;

    public T? First => _count == 0 ? default : FirstEntry.Value;
    public T? Last => _count == 0 ? default : LastEntry.Value;

    public Node FirstNode => _count == 0 ? default : new Node(this, _firstIndex);
    public Node LastNode => _count == 0 ? default : new Node(this, FirstEntry.Prev);

    private ref Entry FirstEntry => ref _entries[_firstIndex];
    private ref Entry LastEntry => ref _entries[FirstEntry.Prev];

    public int Count => _count;

    public ContiguousLinkedList()
    {
        _entries = [];
        _firstIndex = -1;
        _freeFirstIndex = -1;
    }

    public ContiguousLinkedList(int capacity)
    {
        _entries = new Entry[capacity];
        _firstIndex = -1;
        _freeFirstIndex = -1;
    }

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

    public Node AddAfter(Node node, T value)
    {
        ValidateNodeBelonging(node);
        return AddAfterInternal(node._index, value);
    }

    public Node AddBefore(Node node, T value)
    {
        ValidateNodeBelonging(node);
        return AddBeforeInternal(node._index, value);
    }

    public Node AddBefore(ContiguousLinkedListNodeIndex index, T value)
    {
        _ = GetNode(index);
        return AddBeforeInternal(index.Value, value);
    }

    public Node AddAfter(ContiguousLinkedListNodeIndex index,T value)
    {
        _ = GetNode(index);
        return AddAfterInternal(index.Value, value);
    }

    private Node AddAfterInternal(int entryIndex, T value)
    {
        ref readonly var entry = ref _entries[entryIndex];
        Debug.Assert(!entry.IsFreed);

        var index = AddEntry(new Entry
        {
            Value = value,
            Prev = entryIndex,
            Next = entry.Next,
        });
        return new Node(this, index);
    }

    private Node AddBeforeInternal(int entryIndex, T value)
    {
        ref readonly var entry = ref _entries[entryIndex];
        Debug.Assert(!entry.IsFreed);

        var index = AddEntry(new Entry
        {
            Value = value,
            Prev = entry.Prev,
            Next = entryIndex,
        });
        if (entryIndex == _firstIndex)
            _firstIndex = index;
        return new Node(this, index);
    }

    public Node AddFirst(T value)
    {
        if (_count > 0) {
            var index = AddEntry(new Entry
            {
                Value = value,
                Prev = FirstEntry.Prev,
                Next = _firstIndex,
            });
            _firstIndex = index;
            return new Node(this, index);
        }
        else {
            AddToEmptyList(value);
            return new Node(this, 0);
        }
    }

    public Node AddLast(T value)
    {
        if (_count > 0) {
            var index = AddEntry(new Entry
            {
                Value = value,
                Prev = FirstEntry.Prev,
                Next = _firstIndex,
            });
            return new Node(this, index);
        }
        else {
            AddToEmptyList(value);
            return new Node(this, 0);
        }
    }

    /// <returns>Index of added entry</returns>
    private int AddEntry(in Entry entry)
    {
        ref Entry free = ref Unsafe.NullRef<Entry>();
        int index;
        if (_freeFirstIndex >= 0) {
            free = ref _entries[_freeFirstIndex];
            index = _freeFirstIndex;
            _freeFirstIndex = free.Next;
        }
        else {
            if (_count == _entries.Length) {
                ArrayGrowHelper.Grow(ref _entries, _count + 1, _count);
            }
            free = ref _entries[_count];
            index = _count;
            _consumedCount++;
        }
        free = entry with { Version = free.Version + 1 };
        Debug.Assert(!free.IsFreed);
        _entries[entry.Prev].Next = index;
        _entries[entry.Next].Prev = index;
        _count++;
        _version++;
        return index;
    }

    private void AddToEmptyList(T item)
    {
        Debug.Assert(_count == 0);
        if (_entries.Length == 0) {
            ArrayGrowHelper.Grow(ref _entries, 1, 0);
            _consumedCount++;
        }
        ref var free = ref _entries[0];
        free = new Entry
        {
            Value = item,
            Prev = 0,
            Next = 0,
            Version = free.Version + 1,
        };
        Debug.Assert(!free.IsFreed);
        _firstIndex = 0;
        _count++;
        _version++;
    }

    public bool Remove(T value)
    {
        var node = Find(value);
        if (node.HasValue) {
            RemoveEntry(node._index, ref node.Entry);
            return true;
        }
        return false;
    }

    public bool Remove(Node node)
    {
        ValidateNodeBelonging(node);
        if (node.HasValue) {
            RemoveEntry(node._index, ref node.Entry);
            return true;
        }
        return false;
    }

    public bool RemoveAt(ContiguousLinkedListNodeIndex nodeIndex)
    {
        if (TryGetNode(nodeIndex, out var node)) {
            RemoveEntry(node._index, ref node.Entry);
            return true;
        }
        return false;
    }

    public void RemoveFirst()
    {
        if (_count == 0)
            TraThrow.NoElement();
        RemoveEntry(_firstIndex, ref FirstEntry);
    }

    public void RemoveLast()
    {
        if (_count == 0)
            TraThrow.NoElement();
        RemoveEntry(FirstEntry.Prev, ref LastEntry);
    }

    private void RemoveEntry(int index, ref Entry entry)
    {
        Debug.Assert(Unsafe.AreSame(ref _entries[index], ref entry));

        _entries[entry.Prev].Next = entry.Next;
        _entries[entry.Next].Prev = entry.Prev;
        if (index == _firstIndex)
            _firstIndex = entry.Next;
        FreeEntry(index, ref entry);
        _count--;
        if (_count == 0) {
            _firstIndex = -1;
            _consumedCount = 0;
            _freeFirstIndex = -1;
        }
        _version++;
    }

    private void FreeEntry(int index, ref Entry entry)
    {
        Debug.Assert(Unsafe.AreSame(ref _entries[index], ref entry));

        entry = new Entry
        {
            Next = _freeFirstIndex,
            Version = entry.Version + 1
        };
        Debug.Assert(entry.IsFreed);
        _freeFirstIndex = index;
    }

    public void Clear()
    {
        ArrayGrowHelper.FreeManaged(_entries, 0, _consumedCount);
        _firstIndex = -1;
        _count = 0;
        _consumedCount = 0;
        _freeFirstIndex = -1;
        _version++;
    }

    private void ValidateNodeBelonging(UnsafeNode node)
    {
        if (node._list != this)
            TraThrow.NodeBelongsWrong();
    }

    #endregion

    #region Search

    public Node Find(T value)
    {
        var node = FirstNode;
        var first = node._index;

        do {
            if (EqualityComparer<T>.Default.Equals(node.Value, value))
                return node;
            node = node.Next;
        } while (node._index != first);

        return default;
    }

    public Node FindLast(T value)
    {
        var node = LastNode;
        var last = node._index;

        do {
            if (EqualityComparer<T>.Default.Equals(node.Value, value))
                return node;
            node = node.Prev;
        } while (node._index != last);
        return default;
    }

    public bool Contains(T value) => Find(value).HasValue;

    #endregion

    public void EnsureCapacity(int capacity)
    {
        if (capacity < _entries.Length)
            return;
        ArrayGrowHelper.Grow(ref _entries, capacity, _consumedCount);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        TraNumber.ValidateSliceArgs(arrayIndex, _count, array.Length);

        var node = FirstNode;
        var first = node._index;

        do {
            array[arrayIndex++] = node.Value;
            node = node.Next;
        } while (node._index != first);
    }

    public Enumerator GetEnumerator() => new(this);

    public NodesIterator EnumerateNodes() => new(this);

    bool ICollection<T>.IsReadOnly => false;

    void ICollection<T>.Add(T item) => AddLast(item);

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    [DoesNotReturn]
    private static void ThrowNodeInvalidated()
        => ThrowHelper.ThrowInvalidOperationException("The node has been invalidated.");

    internal struct Entry
    {
        public T Value;
        public int Prev;
        public int Next;
        /// <summary>
        /// Version of the entry, change when toggle free status, 
        /// use for validation in node, as we cannot ensure the value at some index
        /// won't be changed if item is removed and then added
        /// </summary>
        /// <remarks>
        /// The last bit is for check if it is in free list, so increment by 1 when free
        /// state toggled is ok
        /// </remarks>
        internal int Version;

        public readonly bool IsFreed => (Version & FreedMask) == 0;

        private const int FreedMask = 0b01;
    }

    /// <summary>
    /// A linked list node for access
    /// </summary>
    /// <remarks>
    /// The node gets item by index, If a node is removed, the associated <see cref="Node"/> or <see cref="ContiguousLinkedListNodeIndex"/> should immediately
    /// set to invalid, or else unexpected data may be get.
    /// </remarks>
    public readonly struct Node : IEquatable<Node>
    {
        internal readonly ContiguousLinkedList<T> _list;
        internal readonly int _index;
        internal readonly int _version;

        public bool HasValue => _list is not null;

        public T Value
        {
            get => Entry.Value;
            set {
                Entry.Value = value;
                _list._version++;
            }
        }

        public ref T ValueRef => ref Entry.Value;

        public Node Next
        {
            get {
                if (!HasValue)
                    return default;
                ref readonly var entry = ref Entry;
                if (entry.Next == _list._firstIndex)
                    return default;
                return new(_list, entry.Next);
            }
        }

        public Node Prev
        {
            get {
                if (!HasValue)
                    return default;
                if (_index == _list._firstIndex)
                    return default;
                ref readonly var entry = ref Entry;
                return new Node(_list, entry.Prev);
            }
        }

        internal ref Entry Entry
        {
            get {
                ref Entry entry = ref DangerousGetEntryRef();
                if (entry.Version != _version)
                    ThrowNodeInvalidated();
                Debug.Assert(!entry.IsFreed);
                return ref entry;
            }
        }

        private ref Entry DangerousGetEntryRef() => ref _list._entries[_index];

        internal Node(ContiguousLinkedList<T> list, int index)
        {
            _list = list;
            _index = index;
            ref readonly var entry = ref _list._entries[_index];
            Debug.Assert(!entry.IsFreed);
            _version = entry.Version;
        }

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

        #region Equality

        public bool Equals(Node other) => _list == other._list && _index == other._index && _version == other._version;
        public override int GetHashCode() => HashCode.Combine(_list, _index, _version);
        public override bool Equals([NotNullWhen(true)] object? obj) => obj is Node node && Equals(node);
        public static bool operator ==(Node left, Node right) => left.Equals(right);
        public static bool operator !=(Node left, Node right) => !left.Equals(right);

        #endregion
    }

    public struct Enumerator : IEnumerator<T>
    {
        private readonly ContiguousLinkedList<T> _list;
        private int _index;
        private readonly int _version;
        private T? _current;

        internal Enumerator(ContiguousLinkedList<T> list)
        {
            _list = list;
            _version = _list._version;
            _index = _list._firstIndex;
        }

        public readonly T Current => _current!;

        readonly object? IEnumerator.Current => Current;

        public bool MoveNext()
        {
            ValidateVersion();

            if (_index < 0)
                return false;

            _current = _list._entries[_index].Value;
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
    }

    public struct NodesIterator : IEnumerable<Node>, IEnumerator<Node>
    {
        private readonly ContiguousLinkedList<T> _list;
        private int _index;
        private readonly int _version;
        private int _current;

        public readonly Node Current => new(_list, _current);

        readonly object IEnumerator.Current => Current;

        internal NodesIterator(ContiguousLinkedList<T> list)
        {
            _list = list;
            _index = _list._firstIndex;
            _version = _list._version;
        }

        public readonly NodesIterator GetEnumerator() => new NodesIterator(_list);

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

        readonly IEnumerator<Node> IEnumerable<Node>.GetEnumerator() => GetEnumerator();
        readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
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
