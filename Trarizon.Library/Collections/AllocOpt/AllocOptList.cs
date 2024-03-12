using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Trarizon.Library.CodeAnalysis;
using Trarizon.Library.Collections.Helpers;

namespace Trarizon.Library.Collections.AllocOpt;
[CollectionBuilder(typeof(AllocOptCollectionBuilder), nameof(AllocOptCollectionBuilder.CreateList))]
public struct AllocOptList<T> : IList<T>, IReadOnlyList<T>
{
    private T[] _items;
    private int _size;

    public AllocOptList()
    {
        _items = [];
    }

    public AllocOptList(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);

        if (capacity == 0)
            _items = [];
        else
            _items = new T[capacity];
    }

    #region Accessors

    public readonly int Count => _size;

    public readonly int Capacity => _items.Length;

    public readonly T this[int index]
    {
        get {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _size);
            return _items[index];
        }
        set {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _size);
            _items[index] = value;
        }
    }

    public readonly Span<T> AsSpan(Range range)
    {
        var (offset, len) = range.GetOffsetAndLength(_size);
        return AsSpan(offset, len);
    }

    public readonly Span<T> AsSpan(int index, int length)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(length, _size - index);
        return _items.AsSpan(index, length);
    }

    public readonly Span<T> AsSpan(int startIndex) => _items.AsSpan(startIndex, _size - startIndex);

    public readonly Span<T> AsSpan() => _items.AsSpan(0, _size);

    public readonly T[] ToArray() => AsSpan().ToArray();

    public readonly T[] GetUnderlyingArray() => _items;

    public readonly Enumerator GetEnumerator() => new(this);

    #endregion

    #region Builders

    public void Add(T item)
    {
        Debug.Assert(_size <= _items.Length);

        if (_size == _items.Length) {
            Grow(_size + 1);
        }
        _items[_size++] = item;
    }

    public void AddRange<TEnumerable>(TEnumerable collection) where TEnumerable : IEnumerable<T>
    {
        if (collection is ICollection<T> c) {
            AddCollection(c);
            return;
        }

        foreach (var item in collection) {
            Add(item);
        }
    }

    public void AddCollection<TCollection>(TCollection collection) where TCollection : ICollection<T>
    {
        int count = collection.Count;
        if (count <= 0)
            return;

        var newSize = _size + count;
        if (newSize > _items.Length) {
            Grow(newSize);
        }
        collection.CopyTo(_items, _size);
        _size = newSize;
    }

    public void AddRange(ReadOnlySpan<T> items)
    {
        int count = items.Length;
        if (count <= 0)
            return;

        var newSize = _size + count;
        if (newSize > _items.Length) {
            Grow(newSize);
        }
        items.CopyTo(_items.AsSpan(_size));
        _size = newSize;
    }

    public void Insert(Index index, T item)
    {
        var offset = index.GetOffset(_size);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(offset, _size);

        if (_size == _items.Length) {
            GrowForInsertion(offset, 1);
        }
        else if (offset < _size) {
            Array.Copy(_items, offset, _items, offset + 1, _size - offset);
        }
        _items[offset] = item;
        _size++;
    }

    public void InsertRange<TEnumerable>(Index index, TEnumerable collection) where TEnumerable : IEnumerable<T>
    {
        var offset = index.GetOffset(_size);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(offset, _size);

        if (collection is ICollection<T> c) {
            InsertCollection(offset, c);
            return;
        }

        foreach (var item in collection) {
            Insert(offset++, item);
        }
    }

    public void InsertCollection<TCollection>(Index index, TCollection collection) where TCollection : ICollection<T>
    {
        int count = collection.Count;
        if (count <= 0)
            return;

        var offset = index.GetOffset(_size);
        var newSize = _size + count;
        if (newSize > _items.Length) {
            GrowForInsertion(offset, count);
        }
        else if (offset < _size) {
            Array.Copy(_items, offset, _items, offset + count, _size - offset);
        }

        collection.CopyTo(_items, offset);
        _size = newSize;
    }

    public void InsertRange(Index index, ReadOnlySpan<T> items)
    {
        int count = items.Length;
        if (count <= 0)
            return;

        var offset = index.GetOffset(_size);
        var newSize = _size + count;
        if (newSize > _items.Length) {
            GrowForInsertion(offset, count);
        }
        else if (offset < _size) {
            Array.Copy(_items, offset, _items, offset + count, _size - offset);
        }

        items.CopyTo(_items.AsSpan(offset));
        _size = newSize;
    }

    [FriendAccess(typeof(AllocOptQueue<>))]
    internal void OverwriteCollectionNonGrow<TCollection>(int index, TCollection collection) where TCollection : ICollection<T>
    {
        collection.CopyTo(_items, index);
        _size = int.Max(_size, index + collection.Count);
    }

    [FriendAccess(typeof(AllocOptQueue<>))]
    internal void OverwriteRangeNonGrow(int index, ReadOnlySpan<T> items)
    {
        items.CopyTo(_items.AsSpan(index, items.Length));
        _size = int.Max(_size, index + items.Length);
    }

    public bool Remove(T item)
    {
        var index = IndexOf(item);
        if (index <= 0)
            return false;

        RemoveAt(index);
        return true;
    }

    public void RemoveAt(Index index)
    {
        var offset = index.GetOffset(_size);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(offset, _items.Length);

        _size--;
        if (offset < _size) {
            Array.Copy(_items, offset + 1, _items, offset, _size - offset);
        }
    }

    public void RemoveRange(Index index, int count)
    {
        var offset = index.GetOffset(_size);
        ArgumentOutOfRangeException.ThrowIfNegative(offset);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(offset + count, _size);

        if (count == 0)
            return;

        _size -= count;
        if (offset < _size) {
            Array.Copy(_items, offset + count, _items, offset, _size - offset);
        }
    }

    public void RemoveRange(Range range)
    {
        var (index, count) = range.GetOffsetAndLength(_size);
        RemoveRange(index, count);
    }

    public int RemoveAll(Predicate<T> predicate)
    {
        int newSize = 0;
        while (newSize < _size && !predicate(_items[newSize]))
            newSize++;
        if (newSize >= _size) // No matched item
            return 0;

        int ptr = newSize + 1;
        while (ptr < _size) {
            while (ptr < _size && !predicate(_items[ptr]))
                ptr++;

            if (ptr < _size) {
                _items[newSize++] = _items[ptr++];
            }
        }

        var freedCount = _size - newSize;
        _size = newSize;
        return freedCount;
    }

    /// <remarks>
    /// This method won't clear elements in underlying array.
    /// Use <see cref="ClearUnreferenced"/> if you need it.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        _size = 0;
    }

    /// <summary>
    /// Clear items from index _size to end
    /// </summary>
    public readonly void ClearUnreferenced()
    {
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            return;

        if (_size < _items.Length) {
            Array.Clear(_items, _size, _items.Length - _size);
        }
    }

    // Unlike List<T>, this does not throw if _size < Capacity
    public void EnsureCapacity(int capacity)
    {
        if (capacity <= _items.Length)
            return;

        Grow(capacity);
    }

    private void Grow(int expectedCapacity)
    {
        GrowEmptyArray(expectedCapacity, out var originalArray);
        Array.Copy(originalArray, _items, _size);
    }

    [FriendAccess(typeof(AllocOptQueue<>))]
    internal void GrowEmptyArray(int expectedCapacity, out T[] originalArray)
    {
        originalArray = _items;
        _items = new T[GetNewCapacity(expectedCapacity)];
    }

    private void GrowForInsertion(int insertIndex, int count)
    {
        Debug.Assert(count > 0);

        int requiredCapacity = checked(_size + count);
        int newCapacity = GetNewCapacity(requiredCapacity);

        T[] newItems = new T[newCapacity];
        if (insertIndex > 0) {
            Array.Copy(_items, newItems, insertIndex);
        }
        if (insertIndex < _size) {
            Array.Copy(_items, insertIndex, newItems, insertIndex + count, _size - insertIndex);
        }
        _items = newItems;
    }

    private readonly int GetNewCapacity(int expectedCapacity)
    {
        Debug.Assert(_items.Length < expectedCapacity);

        int newCapacity;
        if (_items.Length == 0)
            newCapacity = 4;
        else
            newCapacity = int.Min(_items.Length * 2, Array.MaxLength);

        if (newCapacity < expectedCapacity)
            newCapacity = expectedCapacity;

        return int.Max(newCapacity, expectedCapacity);
    }

    #endregion

    #region Interface methods

    public readonly int IndexOf(T item) => Array.IndexOf(_items, item, 0, _size);
    public readonly bool Contains(T item) => _size > 0 && IndexOf(item) >= 0;
    public readonly void CopyTo(T[] array, int arrayIndex) => Array.Copy(_items, 0, array, arrayIndex, _size);

    readonly bool ICollection<T>.IsReadOnly => false;

    void IList<T>.Insert(int index, T item) => Insert(index, item);
    void IList<T>.RemoveAt(int index) => RemoveAt(index);
    readonly IEnumerator<T> IEnumerable<T>.GetEnumerator() => new Enumerator.Wrapper(this);
    readonly IEnumerator IEnumerable.GetEnumerator() => new Enumerator.Wrapper(this);

    #endregion

    public struct Enumerator
    {
        private readonly AllocOptList<T> _list;
        private int _index;

        internal Enumerator(AllocOptList<T> list)
        {
            _list = list;
            _index = -1;
        }

        public readonly T Current => _list[_index];

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

        internal sealed class Wrapper(AllocOptList<T> list) : IEnumerator<T>
        {
            private Enumerator _enumerator = list.GetEnumerator();

            public T Current => _enumerator.Current;

            object? IEnumerator.Current => Current;

            public void Dispose() { }
            public bool MoveNext() => _enumerator.MoveNext();
            public void Reset() => _enumerator._index = -1;
        }
    }
}