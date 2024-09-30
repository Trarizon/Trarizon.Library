using CommunityToolkit.Diagnostics;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Trarizon.Library.Collections.AllocOpt.Providers;
internal struct AllocOptGrowableArrayProvider<T>
{
    private T[] _array;
    private int _count;

    public AllocOptGrowableArrayProvider(int capacity)
    {
        Debug.Assert(capacity >= 0);

        if (capacity == 0)
            _array = [];
        else
            _array = new T[capacity];
    }

    public AllocOptGrowableArrayProvider()
    {
        _array = [];
    }

    public readonly T[] Array => _array;

    public readonly int Count => _count;

    public readonly Span<T> Span => _array.AsSpan(0, Count);

    public void SetCount(int count)
    {
        if (count <= _array.Length) {
            _count = count;
            return;
        }
        else {
            Grow(count);
        }
    }

    public void Add(T item)
    {
        Debug.Assert(_count <= _array.Length);

        if (_count == _array.Length) {
            Grow(_count + 1);
        }
        _array[_count++] = item;
    }

    public void AddRangeEnumerable<TEnumerable>(TEnumerable collection) where TEnumerable : IEnumerable<T>
    {
        if (collection.TryGetNonEnumeratedCount(out var count)) {
            if (count == 0)
                return;
            var newCount = count + Count;
            if (newCount > _array.Length) {
                Grow(Count + count);
            }
            foreach (var item in collection) {
                _array[_count++] = item;
            }
            return;
        }

        foreach (var item in collection) {
            Add(item);
        }
    }

    public void AddRangeSpan(ReadOnlySpan<T> items)
    {
        if (items.Length <= 0)
            return;

        var newCount = _count + items.Length;
        if (newCount > _array.Length) {
            Grow(newCount);
        }
        items.CopyTo(_array.AsSpan(_count, items.Length));
        _count = newCount;
    }

    public void AddRangeCollection<TCollection>(TCollection collection) where TCollection : ICollection<T>
    {
        int count = collection.Count;
        if (count <= 0)
            return;

        var newCount = _count + count;
        if (newCount > _array.Length) {
            Grow(newCount);
        }
        collection.CopyTo(_array, _count);
        _count = newCount;
    }

    public void InsertRangeSpan(int index, ReadOnlySpan<T> items)
    {
        Guard.IsInRange(index, 0, _count);

        int count = items.Length;
        if (count <= 0)
            return;

        ResizeForInsertion(index, count);
        items.CopyTo(_array.AsSpan(index, count));
        _count += count;
    }

    public void InsertRangeCollection(int index, ICollection<T> collection)
    {
        int count = collection.Count;
        if (count <= 0)
            return;

        Guard.IsInRange(index, 0, _count);

        ResizeForInsertion(index, count);
        collection.CopyTo(_array, index);
        _count += count;
    }

    public void InsertRangeEnumerable(int index, IEnumerable<T> collection)
    {
        Guard.IsInRange(index, 0, _count);

        if (collection.TryGetNonEnumeratedCount(out var count)) {
            if (count == 0)
                return;

            ResizeForInsertion(index, count);
            foreach (var item in collection) {
                _array[index++] = item;
            }
            _count += count;
            return;
        }

        foreach (var item in collection) {
            InsertRangeSpan(index++, TraSpan.AsReadOnlySpan(in item));
        }
    }

    public void RemoveRange(int index, int count)
    {
        if (count == 0)
            return;

        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index + count, _count);

        _count -= count;
        if (index < _count) {
            System.Array.Copy(_array, index + count, _array, index, _count - index);
        }
    }

    public void Clear()
    {
        _count = 0;
    }

    /// <summary>
    /// Clear items from index <see cref="Count"/> to end
    /// </summary>
    public readonly void FreeUnreferenced()
    {
#if NET8_0_OR_GREATER
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            return;
#endif
        if (Count < _array.Length) {
            System.Array.Clear(_array, Count, _array.Length - Count);
        }
    }

    public void EnsureCapacity(int capacity)
    {
        if (capacity < _array.Length)
            return;

        Grow(capacity);
    }

    private void ResizeForInsertion(int index, int count)
    {
        var newSize = _count + count;
        if (newSize > _array.Length) {
            GrowForInsertion(index, count);
        }
        else if (index < _count) {
            System.Array.Copy(_array, index, _array, index + count, _count - index);
        }
    }

    /// <param name="expectedCapacity">Greater than capacity</param>
    private void Grow(int expectedCapacity)
    {
        GrowNonMove(expectedCapacity, out var originalArray);
        System.Array.Copy(originalArray, _array, _count);
    }

    private void GrowForInsertion(int insertIndex, int insertCount)
    {
        Debug.Assert(insertCount > 0);

        GrowNonMove(checked(_count + insertCount), out var originalArray);

        if (insertIndex > 0) {
            System.Array.Copy(originalArray, _array, insertIndex);
        }
        if (insertIndex < _count) {
            System.Array.Copy(originalArray, insertIndex, _array, insertIndex + insertCount, _count - insertIndex);
        }
    }

    private void GrowNonMove(int expectedCapacity, out T[] originalArray)
    {
        originalArray = _array;
        _array = new T[GetNewCapacity(expectedCapacity)];
    }

    private readonly int GetNewCapacity(int expectedCapacity)
    {
        Debug.Assert(_array.Length < expectedCapacity);

        int newCapacity;
        if (_array.Length == 0)
            newCapacity = 4;
        else {
            newCapacity = int.Min(_array.Length * 2, System.Array.MaxLength);
        }

        if (newCapacity < expectedCapacity)
            newCapacity = expectedCapacity;
        return newCapacity;
    }
}
