using CommunityToolkit.Diagnostics;
using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Trarizon.Library.Numerics;

namespace Trarizon.Library.Collections.Generic;
[CollectionBuilder(typeof(CollectionBuilders), nameof(CollectionBuilders.CreateContiguousLinkedList))]
public class ContiguousLinkedList<T> : ICollection<T>
{
    private Entry[] _entries;
    private int _firstIndex;
    private int _count;
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

    public int Capacity
    {
        get => _entries.Length;
        set {
            Guard.IsGreaterThanOrEqualTo(value, _count);
            if (value == _count)
                return;
            Entry[] array = new Entry[value];
            if (_count > 0) {
                Array.Copy(_entries, array, _count);
            }
            _entries = array;
        }
    }

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

    #region Modification

    public void AddAfter(Node node, T value)
    {
        AddEntry(new Entry
        {
            Value = value,
            Prev = node._index,
            Next = node.Entry.Next,
        });
    }

    public void AddBefore(Node node, T value)
    {
        AddEntry(new Entry
        {
            Value = value,
            Prev = node.Entry.Prev,
            Next = node._index,
        });
        if (node._index == _firstIndex)
            _firstIndex = node.Entry.Prev;
    }

    public void AddFirst(T value)
    {
        if (_count > 0) {
            AddEntry(new Entry
            {
                Value = value,
                Prev = FirstEntry.Prev,
                Next = _firstIndex,
            });
            _firstIndex = FirstEntry.Prev;
        }
        else {
            AddToEmptyList(value);
        }
    }

    public void AddLast(T value)
    {
        if (_count > 0) {
            AddEntry(new Entry
            {
                Value = value,
                Prev = FirstEntry.Prev,
                Next = _firstIndex,
            });
        }
        else {
            AddToEmptyList(value);
        }
    }

    private void AddEntry(in Entry entry)
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
        }
        free = entry;
        _entries[entry.Prev].Next = index;
        _entries[entry.Next].Prev = index;
        _count++;
        _version++;
    }

    private void AddToEmptyList(T item)
    {
        Debug.Assert(_count == 0);
        if (_entries.Length == 0)
            ArrayGrowHelper.Grow(ref _entries, 1, 0);
        ref var free = ref _entries[0];
        free = new Entry
        {
            Value = item,
            Prev = 0,
            Next = 0,
        };
        _firstIndex = 0;
        _count++;
        _version++;
    }

    public bool Remove(T value)
    {
        var node = Find(value);
        if (node.HasValue) {
            RemoveNodeInternal(node);
            return true;
        }
        return false;
    }

    public bool Remove(Node node)
    {
        ValidateNode(node);
        if (node.HasValue) {
            RemoveNodeInternal(node);
            return true;
        }
        return false;
    }

    public void RemoveFirst()
    {
        if (_count == 0)
            TraThrow.NoElement();
        RemoveNodeInternal(FirstNode);
    }

    public void RemoveLast()
    {
        if (_count == 0)
            TraThrow.NoElement();
        RemoveNodeInternal(LastNode);
    }

    private void RemoveNodeInternal(in Node node)
    {
        Debug.Assert(node._list == this);

        ref var entry = ref node.Entry;
        _entries[entry.Prev].Next = entry.Next;
        _entries[entry.Next].Prev = entry.Prev;
        if (node._index == _firstIndex)
            _firstIndex = entry.Next;
        FreeNode(node._index);
        _count--;
        _version++;
    }

    private void FreeNode(int index)
    {
        ref var free = ref _entries[index];
        free = new Entry { Next = _freeFirstIndex };
        _freeFirstIndex = index;
    }

    public void Clear()
    {
        ArrayGrowHelper.FreeManaged(_entries);
        _firstIndex = -1;
        _count = 0;
        _freeFirstIndex = -1;
        _version++;
    }

    #endregion

    public void EnsureCapacity(int capacity)
    {
        if (capacity < _entries.Length)
            return;
        ArrayGrowHelper.Grow(ref _entries, capacity, _count);
    }

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

    private void ValidateNode(Node node)
    {
        if (node._list != this)
            TraThrow.NodeBelongsWrong();
    }

    bool ICollection<T>.IsReadOnly => false;

    void ICollection<T>.Add(T item) => AddLast(item);

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    internal struct Entry
    {
        public T Value;
        public int Prev;
        public int Next;
    }

    public readonly struct Node : IEquatable<Node>
    {
        internal readonly ContiguousLinkedList<T> _list;
        internal readonly int _index;

        public bool HasValue => _list is not null;

        internal ref Entry Entry => ref _list._entries[_index];

        public T Value
        {
            get => Entry.Value;
            set => Entry.Value = value;
        }

        public Node Next => new Node(_list, Entry.Next);

        public Node Prev => new Node(_list, Entry.Prev);

        internal Node(ContiguousLinkedList<T> list, int index)
        {
            _list = list;
            _index = index;
        }

        public ref T GetValueRef() => ref Entry.Value;

        public bool Equals(Node other) => _list == other._list && _index == other._index;

        public override bool Equals(object? obj)
        {
            return obj is Node node && Equals(node);
        }

        public override int GetHashCode() => HashCode.Combine(_list, _index);

        public static bool operator ==(Node left, Node right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Node left, Node right)
        {
            return !(left == right);
        }
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

        public void Reset() => throw new NotImplementedException();
        readonly void IDisposable.Dispose() { }
    }
}
