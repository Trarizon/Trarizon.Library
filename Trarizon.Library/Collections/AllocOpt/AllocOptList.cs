using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Trarizon.Library.Collections.AllocOpt;
using Trarizon.Library.Collections.Extensions;

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
        ArgumentOutOfRangeException.ThrowIfLessThan(capacity, 0);

        if (capacity == 0)
            _items = [];
        else
            _items = new T[capacity];
    }

    public readonly int Count => _size;

    public int Capacity
    {
        readonly get => _items.Length;
        // Unlike List<T>, this does not throw if _size < Capacity
        set => EnsureCapacity(value);
    }

    readonly bool ICollection<T>.IsReadOnly => false;

    #region Accessors

    public readonly T this[int index]
    {
        get => _items[index];
        set => _items[index] = value;
    }

    public readonly Span<T> Slice(int index, int length) => _items.AsSpan(index, length);

    public readonly Span<T> AsSpan() => _items.AsSpan(0, _size);

    public readonly T[] ToArray()
    {
        if (_size == 0)
            return [];

        T[] result = new T[_size];
        Array.Copy(_items, result, _size);
        return result;
    }

    public readonly T[] GetUnderlyingArray() => _items;

    #endregion

    #region Building

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

    public void AddRange(ReadOnlySpan<T> values)
    {
        int count = values.Length;
        if (count <= 0)
            return;

        var newSize = _size + count;
        if (newSize > _items.Length) {
            Grow(newSize);
        }
        values.CopyTo(_items.AsSpan(_size));
        _size = newSize;
    }

    public void Insert(int index, T item)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(index, _size);

        if (_size == _items.Length) {
            GrowForInsertion(index, 1);
        }
        else if (index < _size) {
            Array.Copy(_items, index, _items, index + 1, _size - index);
        }
        _items[index] = item;
        _size++;
    }

    public void InsertRange<TEnumerable>(int index, TEnumerable collection) where TEnumerable : IEnumerable<T>
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(index, _size);

        if (collection is ICollection<T> c) {
            InsertCollection(index, c);
            return;
        }

        foreach (var item in collection) {
            Insert(index++, item);
        }
    }

    public void InsertCollection<TCollection>(int index, TCollection collection) where TCollection : ICollection<T>
    {
        int count = collection.Count;
        if (count <= 0)
            return;

        var newSize = _size + count;
        if (newSize > _items.Length) {
            GrowForInsertion(index, count);
        }
        else if (index < _size) {
            Array.Copy(_items, index, _items, index + count, _size - index);
        }

        collection.CopyTo(_items, index);
        _size = newSize;
    }

    public void InsertRange(int index, ReadOnlySpan<T> values)
    {
        int count = values.Length;
        if (count <= 0)
            return;

        var newSize = _size + count;
        if (newSize > _items.Length) {
            GrowForInsertion(index, count);
        }
        else if (index < _size) {
            Array.Copy(_items, index, _items, index + count, _size - index);
        }

        values.CopyTo(_items.AsSpan(index));
        _size = newSize;
    }

    public bool Remove(T item)
    {
        var index = IndexOf(item);
        if (index <= 0)
            return false;

        RemoveAt(index);
        return true;
    }

    public void RemoveAt(int index)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _items.Length);

        _size--;
        if (index < _size) {
            Array.Copy(_items, index + 1, _items, index, _size - index);
        }
    }

    public void RemoveRange(int index, int count)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(index, 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(count, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index + count, _size);

        if (count == 0)
            return;

        _size -= count;
        if (index < _size) {
            Array.Copy(_items, index + count, _items, index, _size - index);
        }
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
    /// <see cref="AllocOptList{T}"/> is designed for stack, so this method
    /// will not clear the element of underlying array.
    /// Use <see cref="ClearTrailingReferences"/> if you need it.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        _size = 0;
    }

    /// <summary>
    /// Clear items from index _size to end
    /// </summary>
    public void ClearTrailingReferences()
    {
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>()) {
            if (_size > 0) {
                Array.Clear(_items);
                _size = 0;
            }
        }
        else {
            Clear();
        }
    }

    #endregion

    public void EnsureCapacity(int capacity)
    {
        if (capacity <= _items.Length)
            return;

        Grow(capacity);
    }

    private void Grow(int expectedCapacity)
    {
        T[] newItems = new T[GetNewCapacity(expectedCapacity)];
        Array.Copy(_items, newItems, _size);
        _items = newItems;
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

    public readonly int IndexOf(T item) => Array.IndexOf(_items, item, 0, _size);
    public readonly bool Contains(T item) => _size > 0 && IndexOf(item) >= 0;
    public readonly void CopyTo(T[] array, int arrayIndex) => Array.Copy(_items, 0, array, arrayIndex, _size);
    public readonly Enumerator GetEnumerator() => new(this);
    readonly IEnumerator<T> IEnumerable<T>.GetEnumerator() => new Enumerator.Wrapper(this);
    readonly IEnumerator IEnumerable.GetEnumerator() => new Enumerator.Wrapper(this);

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
            private Enumerator enumerator = list.GetEnumerator();

            public T Current => enumerator.Current;

            object? IEnumerator.Current => Current;

            public void Dispose() { }
            public bool MoveNext() => enumerator.MoveNext();
            public void Reset() => enumerator._index = -1;
        }
    }
}
