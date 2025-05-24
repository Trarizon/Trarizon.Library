using CommunityToolkit.Diagnostics;
using System.Collections;
using System.Diagnostics;
using Trarizon.Library.Collections.Helpers;

namespace Trarizon.Library.Collections.Generic;
public sealed class SortedList<T> : IList<T>, IReadOnlyList<T>
{
    private T[] _array;
    private int _count;
    private int _version;
    private IComparer<T> _comparer;

    public T this[int index]
    {
        get {
            Guard.IsLessThan((uint)index, (uint)_count);
            return _array[index];
        }
        set {
            Guard.IsLessThan((uint)index, (uint)_count);
            _array[index] = value;
            NotifyEditedAt(index);
        }
    }

    public int Count => _count;

    public int Capacity
    {
        get => _array.Length;
        set {
            Guard.IsGreaterThanOrEqualTo(value, _count);
            if (value == _count)
                return;
            T[] array = new T[value];
            if (_count > 0) {
                _array.AsSpan(.._count).CopyTo(array);
            }
            _array = array;
        }
    }

    public IComparer<T> Comparer
    {
        get => _comparer;
        set {
            _comparer = value;
            Resort();
        }
    }

    public SortedList(IComparer<T> comparer)
    {
        _comparer = comparer;
        _array = Array.Empty<T>();
    }

    public SortedList(int capacity, IComparer<T> comparer)
    {
        Guard.IsGreaterThanOrEqualTo(capacity, 0);
        _comparer = comparer;
        _array = capacity == 0 ? [] : new T[capacity];
    }

    public ReadOnlySpan<T> AsSpan() => _array.AsSpan(0, _count);

    public ReadOnlySpan<T> AsSpan(int start, int length)
    {
        TraIndex.ValidateSliceArgs(start, length, _count);
        return _array.AsSpan(start, length);
    }

    public ReadOnlySpan<T> AsSpan(Range range)
    {
        var (ofs, len) = range.GetOffsetAndLength(_count);
        return AsSpan(ofs, len);
    }

    #region Search

    public int BinarySearch(T item) => _array.AsSpan(0, _count).BinarySearch(item, _comparer);

    public int BinarySearch(Range priorRange, T item)
        => TraSpan.BinarySearchRangePriority(AsSpan(), priorRange, new TraComparison.ComparerComparable<T, IComparer<T>>(item, _comparer));

    public int LinearSearch(T item) => _array.AsSpan(0, _count).LinearSearch(item, _comparer);

    public int LinearSearchFromEnd(T item) => _array.AsSpan(0, _count).LinearSearchFromEnd(item, _comparer);

    public int LinearSearch(Index nearIndex, T item)
        => TraSpan.LinearSearchFromNear(AsSpan(), nearIndex.GetOffset(_count), new TraComparison.ComparerComparable<T, IComparer<T>>(item, _comparer));

    public bool Contains(T item) => BinarySearch(item) >= 0;

    #endregion

    #region Add

    /// <summary>
    /// Binary search the index and insert with keeping the list in order
    /// </summary>
    public void Add(T item)
    {
        var index = BinarySearch(item);
        TraIndex.FlipNegative(ref index);
        InsertAt(index, item);
    }

    public void Add(Index nearIndex, T item)
    {
        var index = LinearSearch(nearIndex, item);
        TraIndex.FlipNegative(ref index);
        InsertAt(index, item);
    }

    public void Add(Range priorRange, T item)
    {
        var index = BinarySearch(priorRange, item);
        TraIndex.FlipNegative(ref index);
        InsertAt(index, item);
    }

    #endregion

    #region Remove

    public int Remove(T item)
    {
        var index = BinarySearch(item);
        if (index >= 0) {
            RemoveAt(index);
        }
        return index;
    }

    public int Remove(T item, Index nearIndex)
    {
        var index = LinearSearch(nearIndex, item);
        if (index >= 0)
            RemoveAt(index);
        return index;
    }

    public int Remove(T item, Range guessRange)
    {
        var index = BinarySearch(guessRange, item);
        if (index >= 0)
            RemoveAt(index);
        return index;
    }

    public void RemoveAt(int index)
    {
        Guard.IsLessThan((uint)index, (uint)_count);

        ArrayGrowHelper.ShiftLeftForRemoveAndFree(_array, _count, index, 1);
        _count--;
        _version++;
    }

    public void RemoveAt(Index index) => RemoveAt(index.GetOffset(_count));

    public void RemoveRange(int start, int length)
    {
        TraIndex.ValidateSliceArgs(start, length, _count);

        ArrayGrowHelper.ShiftLeftForRemoveAndFree(_array, _count, start, length);
        _count -= length;
        _version++;
    }

    public void RemoveRange(Range range)
    {
        var (ofs, len) = range.GetOffsetAndLength(_count);
        RemoveRange(ofs, len);
    }

    public void Clear()
    {
        ArrayGrowHelper.FreeIfReferenceOrContainsReferences(_array.AsSpan(0, _count));
        _count = 0;
        _version++;
    }

    #endregion

    private void InsertAt(int index, T item)
    {
        Debug.Assert(_count <= _array.Length);
        if (_count == _array.Length) {
            ArrayGrowHelper.GrowForInsertion(ref _array, _count + 1, _count, index, 1);
        }
        else {
            ArrayGrowHelper.ShiftRightForInsert(_array, _count, index, 1);
        }
        _array[index] = item;
        _count++;
        _version++;
    }

    private void MoveTo(int from, int to)
    {
        Debug.Assert((uint)from < (uint)_count);
        Debug.Assert((uint)to < (uint)_count);

        _array.MoveTo(from, to);
    }

    public void EnsureCapacity(int capacity)
    {
        if (capacity < _array.Length)
            return;
        ArrayGrowHelper.Grow(ref _array, capacity, _count);
    }

    public void Resort()
    {
        Array.Sort(_array, 0, _count, _comparer);
    }

    public void ResortBubble()
    {
        TraAlgorithm.BubbleSort(_array.AsSpan(0, _count), _comparer);
    }

    private void NotifyEditedAt(int index)
    {
        Debug.Assert(index < _count);

        var editItem = _array[index];
        if (index >= 1 && _comparer.Compare(_array[index - 1], editItem) > 0) {
            var destIndex = _array.AsSpan(0, index).BinarySearch(editItem, _comparer);
            TraIndex.FlipNegative(ref destIndex);
            MoveTo(index, destIndex);
        }
        else if (index < _count - 1 && _comparer.Compare(editItem, _array[index + 1]) > 0) {
            var destIndex = _array.AsSpan(index + 1, _count - 1 - index).BinarySearch(editItem, _comparer);
            TraIndex.FlipNegative(ref destIndex);
            MoveTo(index, index + destIndex);
        }
        else {
            // No Move
        }
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        TraIndex.ValidateSliceArgs(arrayIndex, _count, array.Length);
        AsSpan().CopyTo(array.AsSpan(arrayIndex, _count));
    }

    public Enumerator GetEnumerator() => new(this);

    #region Interface

    bool ICollection<T>.IsReadOnly => false;

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    int IList<T>.IndexOf(T item) => Math.Max(BinarySearch(item), -1);
    void IList<T>.Insert(int index, T item) => ThrowHelper.ThrowInvalidOperationException("Cannot insert at specific position to SortedList");
    bool ICollection<T>.Remove(T item) => Remove(item) >= 0;
    void ICollection<T>.Add(T item) => Add(item);

    #endregion

    public struct Enumerator : IEnumerator<T>
    {
        private readonly SortedList<T> _list;
        private int _index;
        private readonly int _version;
        private T _current;

        internal Enumerator(SortedList<T> list)
        {
            _list = list;
            _index = 0;
            _version = _list._version;
            _current = default!;
        }

        public readonly T Current => _current;

        readonly object IEnumerator.Current => Current!;

        public bool MoveNext()
        {
            CheckVersion();

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
            CheckVersion();
            _index = 0;
        }

        readonly void IDisposable.Dispose() { }

        private readonly void CheckVersion()
        {
            if (_version != _list._version)
                Throws.CollectionModifiedAfterEnumeratorCreated();
        }
    }

    public readonly struct TrackingScope : IDisposable
    {
        private readonly SortedList<T> _list;
        private readonly int _index;
        private readonly T _item;

        internal TrackingScope(SortedList<T> list, int index, T item)
        {
            Debug.Assert(!typeof(T).IsValueType);
            Guard.IsInRangeFor(index, list.AsSpan());
            _list = list;
            _index = index;
            _item = item;
        }

        public void Dispose()
        {
            if (!ReferenceEquals(_item, _list[_index])) {
                ThrowHelper.ThrowInvalidOperationException("The item is moved after tracking scope created");
            }
            _list.NotifyEditedAt(_index);
        }
    }
}

public static class SortedListExtensions
{
    /// <summary>
    /// Track <paramref name="item"/> in list, when dispose, the index of <paramref name="item"/> in 
    /// list will be auto-adjusted
    /// </summary>
    public static SortedList<T>.TrackingScope EnterTrackingScope<T>(this SortedList<T> list, T item) where T : class
    {
        var index = list.BinarySearch(item);
        return new SortedList<T>.TrackingScope(list, index, item);
    }

    /// <summary>
    /// Track item at <paramref name="index"/> in list, when dispose, the index of item in 
    /// list will be auto-adjusted
    /// </summary>
    public static SortedList<T>.TrackingScope EnterTrackingScope<T>(this SortedList<T> list, int index) where T : class
    {
        return new SortedList<T>.TrackingScope(list, index, list[index]);
    }
}
