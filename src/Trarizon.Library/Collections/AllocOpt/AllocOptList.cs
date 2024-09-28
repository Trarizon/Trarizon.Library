using CommunityToolkit.Diagnostics;
using System.Runtime.CompilerServices;
using Trarizon.Library.Collections.AllocOpt.Providers;

namespace Trarizon.Library.Collections.AllocOpt;
public struct AllocOptList<T>
{
    private AllocOptGrowableArrayProvider<T> _items;

    public AllocOptList()
    {
        _items = new();
    }

    public AllocOptList(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);

        if (capacity == 0)
            _items = new();
        else
            _items = new(capacity);
    }

    public readonly int Count => _items.Count;

    public readonly int Capacity => _items.Array.Length;

    public readonly T this[int index]
    {
        get {
            Guard.IsInRange(index, 0, Count);
            return _items.Array[index];
        }
        set {
            Guard.IsInRange(index, 0, Count);
            _items.Array[index] = value;
        }
    }

    public void Add(T item) => _items.Add(item);

    public void AddRange(ReadOnlySpan<T> items) => _items.AddRangeSpan(items);

    public void AddRange(IEnumerable<T> items)
    {
        if (items is ICollection<T> collection)
            _items.AddRangeCollection(collection);
        else
            _items.AddRangeEnumerable(items);
    }

    public void Insert(Index index, T item)
        => _items.InsertRangeSpan(index.GetOffset(Count), TraSpan.AsReadOnlySpan(in item));

    public void InsertRange(Index index, ReadOnlySpan<T> items)
        => _items.InsertRangeSpan(index.GetOffset(Count), items);

    public void InsertRange(Index index, IEnumerable<T> items)
    {
        if (items is ICollection<T> collection)
            _items.InsertRangeCollection(index.GetOffset(Count), collection);
        else
            _items.InsertRangeEnumerable(index.GetOffset(Count), items);
    }

    public bool Remove(T item)
    {
        var index = Array.IndexOf(_items.Array, item, 0, Count);
        if (index < 0)
            return false;

        _items.RemoveRange(index, 1);
        return true;
    }

    public void RemoveAt(Index index)
        => _items.RemoveRange(index.GetOffset(Count), 1);

    public void RemoveRange(Index index, int count)
        => _items.RemoveRange(index.GetOffset(Count), count);

    public void RemoveRange(Range range)
    {
        var (index, count) = range.GetOffsetAndLength(Count);
        _items.RemoveRange(index, count);
    }

    public int RemoveAll(Predicate<T> predicate)
    {
        int newSize = 0;
        while (newSize < Count && !predicate(this[newSize]))
            newSize++;
        if (newSize >= Count) // No matched item
            return 0;

        int ptr = newSize + 1;
        while (ptr < Count) {
            while (ptr < Count && !predicate(this[ptr]))
                ptr++;

            if (ptr < Count) {
                this[newSize++] = this[ptr++];
            }
        }

        var freedCount = Count - newSize;
        _items.SetCount(newSize);
        return freedCount;
    }

    public void Clear() => _items.SetCount(0);

    /// <summary>
    /// Clear items from index <see cref="Count"/> to end
    /// </summary>
    public readonly void FreeUnreferenced()
    {
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            return;

        if (Count < Capacity) {
            Array.Clear(_items.Array, Count, Capacity - Count);
        }
    }

    /// <summary>
    /// Ensure capacity of collection is greater than <paramref name="capacity"/>,
    /// unlike <see cref="List{T}.EnsureCapacity(int)"/>, this method doesn't throw if <paramref name="capacity"/> less than current
    /// </summary>
    public void EnsureCapacity(int capacity) => _items.EnsureCapacity(capacity);

    public readonly Enumerator GetEnumerator() => new(this);

    public struct Enumerator
    {
        private readonly AllocOptList<T> _list;
        private int _index;

        internal Enumerator(AllocOptList<T> list)
        {
            _list = list;
            _index = 0;
        }

        public readonly T Current
        {
            get {
                if (_index >= 0 && _index < _list.Count)
                    return _list[_index];
                return default!;
            }
        }

        public bool MoveNext()
        {
            var index = _index + 1;
            if (index < _list.Count) {
                _index = index;
                return true;
            }
            else {
                return false;
            }
        }
    }
}
