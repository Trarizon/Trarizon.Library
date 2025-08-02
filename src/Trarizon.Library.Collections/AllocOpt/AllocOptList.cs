using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Trarizon.Library.Collections.Helpers;
#if NETSTANDARD2_0
using RuntimeHelpers = Trarizon.Library.Collections.Helpers.PfRuntimeHelpers;
#endif

namespace Trarizon.Library.Collections.AllocOpt;
/// <summary>
/// Minimal struct list use pooled array as underlying array
/// </summary>
/// <typeparam name="T"></typeparam>
[CollectionBuilder(typeof(CollectionBuilders), nameof(CollectionBuilders.CreateAllocOptList))]
public struct AllocOptList<T> : IDisposable
{
    private T[] _array;
    private int _count;

    public AllocOptList()
    {
        _array = [];
    }

    public AllocOptList(int minInitialCapacity)
    {
        Throws.ThrowIfNegative(minInitialCapacity);

        if (minInitialCapacity == 0)
            _array = [];
        else
            _array = ArrayPool<T>.Shared.Rent(minInitialCapacity);
    }

    #region Accessors

    public readonly int Count => _count;

    public readonly int Capacity => _array.Length;

    public readonly T this[int index]
    {
        get {
            Throws.ThrowIfIndexGreaterThanOrEqual(index, _count);
            return Unsafes.GetReferenceAt(_array, index);
        }
        set {
            Throws.ThrowIfIndexGreaterThanOrEqual(index, _count);
            Unsafes.GetReferenceAt(_array, index) = value;
        }
    }

    public readonly T[] GetUnderlyingArray() => _array;

    public readonly Span<T> AsSpan() => _array.AsSpan(0, _count);

    public readonly Span<T> AsSpan(int start, int length) => _array.AsSpan(start, length);

    public readonly Span<T> AsSpan(Range range)
    {
        var (ofs, len) = range.GetOffsetAndLength(_count);
        return AsSpan(ofs, len);
    }

    public readonly T[] ToArray() => AsSpan().ToArray();

    #endregion

    #region Modification

    public void Add(T item)
    {
        Debug.Assert(_count <= _array.Length);
        if (_count == _array.Length) {
            ArrayGrowHelper.GrowPooled(ref _array, _count + 1, _count);
        }
        _array[_count++] = item;
    }

    public void AddRange(IEnumerable<T> items)
    {
        if (items is ICollection<T> collection) {
            int count = collection.Count;
            if (count <= 0)
                return;

            var newCount = _count + count;
            if (newCount > _array.Length) {
                ArrayGrowHelper.GrowPooled(ref _array, newCount, _count);
            }
            collection.CopyTo(_array, _count);
            _count = newCount;
            return;
        }

        foreach (var item in items) {
            Add(item);
        }
    }

    public void AddRange(ReadOnlySpan<T> items)
    {
        if (items.Length <= 0)
            return;

        var newCount = _count + items.Length;
        if (newCount > _array.Length) {
            ArrayGrowHelper.GrowPooled(ref _array, newCount, _count);
        }
        items.CopyTo(_array.AsSpan(_count, items.Length));
        _count = newCount;
    }

    public void Insert(int index, T item)
    {
        Throws.ThrowIfIndexGreaterThanOrEqual(index, _count);

        EnsureArrayForInsertion(index, 1);
        _array[index] = item;
        _count++;
    }

    public void InsertRange(int index, IEnumerable<T> items)
    {
        Throws.ThrowIfIndexGreaterThanOrEqual(index, _count);

        if (items.TryGetNonEnumeratedCount(out var count)) {
            if (count <= 0)
                return;
            EnsureArrayForInsertion(index, count);
            if (items is ICollection<T> collection) {
                collection.CopyTo(_array, index);
            }
            else {
                foreach (var item in items) {
                    _array[index++] = item;
                }
            }
            _count += count;
            return;
        }
        else {
            foreach (var item in items) {
                Insert(index++, item);
            }
            return;
        }
    }

    public void InsertRange(int index, ReadOnlySpan<T> items)
    {
        Throws.ThrowIfIndexGreaterThanOrEqual(index, _count);

        int count = items.Length;
        if (count <= 0)
            return;

        EnsureArrayForInsertion(index, count);
        items.CopyTo(_array.AsSpan(index, count));
        _count += count;
    }

    private void EnsureArrayForInsertion(int index, int insertCount)
    {
        var newSize = _count + insertCount;
        if (newSize > _array.Length) {
            ArrayGrowHelper.GrowPooledForInsertion(ref _array, newSize, _count, index, insertCount);
        }
        else {
            ArrayGrowHelper.ShiftRightForInsert(_array, _count, index, insertCount);
        }
    }

    public bool Remove(T item)
    {
        var index = Array.IndexOf(_array, item, 0, _count);
        if (index < 0)
            return false;

        ArrayGrowHelper.ShiftLeftForRemoveAndFree(_array, _count, index, 1);
        _count--;
        return true;
    }

    public void RemoveAt(int index)
    {
        Throws.ThrowIfIndexGreaterThanOrEqual(index, _count);

        ArrayGrowHelper.ShiftLeftForRemoveAndFree(_array, _count, index, 1);
        _count--;
    }

    public void RemoveRange(int index, int count)
    {
        Throws.ThrowIfNegative(index);
        Throws.ThrowIfNegative(count);
        Throws.ThrowIfGreaterThanOrEqual(index + count, _count);

        ArrayGrowHelper.ShiftLeftForRemoveAndFree(_array, _count, index, count);
        _count -= count;
    }

    public void RemoveRange(Range range)
    {
        var (index, count) = range.GetOffsetAndLength(_count);

        ArrayGrowHelper.ShiftLeftForRemoveAndFree(_array, _count, index, count);
        _count -= count;
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

        var freedCount = _count - newSize;
        _count = newSize;
        return freedCount;
    }

    /// <summary>
    /// The method does not free reference.
    /// </summary>
    public void Clear() => _count = 0;

    #endregion

    /// <summary>
    /// Ensure capacity of collection is greater than <paramref name="capacity"/>,
    /// this method doesn't throw if <paramref name="capacity"/> less than current
    /// </summary>
    public void EnsureCapacity(int capacity)
    {
        if (capacity < _array.Length)
            return;
        ArrayGrowHelper.GrowPooled(ref _array, capacity, _count);
    }

    public void Dispose()
    {
        if (_array.Length > 0) {
            ArrayPool<T>.Shared.Return(_array, RuntimeHelpers.IsReferenceOrContainsReferences<T>());
        }
        _array = null!;
    }

    public readonly Enumerator GetEnumerator() => new(this);

    public struct Enumerator
    {
        private readonly AllocOptList<T> _list;
        private int _index;
        private T _current;

        internal Enumerator(AllocOptList<T> list)
        {
            _list = list;
            _index = 0;
            _current = default!;
        }

        public readonly T Current => _current;

        public bool MoveNext()
        {
            if (_index < 0)
                return false;

            if (_index < _list.Count) {
                _current = _list[_index];
                _index++;
                return true;
            }
            else {
                _index = -1;
                _current = default!;
                return false;
            }
        }

        public void Reset()
        {
            _index = 0;
        }
    }
}
