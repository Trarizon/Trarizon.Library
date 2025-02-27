using CommunityToolkit.Diagnostics;
using System.Collections;
using System.Diagnostics;
using Trarizon.Library.Numerics;

namespace Trarizon.Library.Collections.Generic;
public sealed class SortedList<T> : IList<T>, IReadOnlyList<T>
{
    private T[] _array;
    private int _count;
    private int _version;
    private readonly IComparer<T> _comparer;

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

    public int Capacity => _array.Length;

    bool ICollection<T>.IsReadOnly => throw new NotImplementedException();

    public SortedList(IComparer<T> comparer)
    {
        _comparer = comparer;
        _array = Array.Empty<T>();
    }

    public SortedList(int capacity, IComparer<T> comparer)
    {
        _comparer = comparer;
        _array = new T[capacity];
    }

    public ReadOnlySpan<T> AsSpan() => _array.AsSpan(0, _count);

    public int BinarySearch(T item) => AsSpan().BinarySearch(item, _comparer);

    public void Add(T item)
    {
        var index = BinarySearch(item);
        TraNumber.FlipNegative(ref index);
        InsertAt(index, item);
        _count++;

    }

    public void AddFromEnd(T item)
    {
        var index = BinarySearch(item);
        TraNumber.FlipNegative(ref index);
        InsertAt(index, item);
    }

    public void AddFromStart(T item)
    {
        var index = BinarySearch(item);
        TraNumber.FlipNegative(ref index);
        InsertAt(index, item);
    }

    public int Remove(T item)
    {
        var index = BinarySearch(item);
        if (index >= 0) {
            RemoveAt(index);
        }
        return index;
    }

    public void RemoveAt(int index)
    {
        Guard.IsLessThan((uint)index, (uint)_count);

        Array.Copy(_array, index + 1, _array, index, _count - index - 1);
        ArrayGrowHelper.FreeManaged(_array, _count, 1);
        _count--;
        _version++;
    }

    public void Clear()
    {
        ArrayGrowHelper.FreeManaged(_array, 0, _count);
        _count = 0;
        _version++;
    }

    private void InsertAt(int index, T item)
    {
        Debug.Assert(_count <= _array.Length);
        if (_count == _array.Length) {
            ArrayGrowHelper.GrowForInsertion(ref _array, _count + 1, _count, index, 1);
        }
        else {
            Array.Copy(_array, index, _array, index + 1, _count - index);
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

    private void NotifyEditedAt(int index)
    {
        Debug.Assert(index < _count);

        var editItem = _array[index];
        if (index >= 1 && _comparer.Compare(_array[index - 1], editItem) > 0) {
            var destIndex = _array.AsSpan(0, index).BinarySearch(editItem, _comparer);
            TraNumber.FlipNegative(ref destIndex);
            MoveTo(index, destIndex);
        }
        else if (index < _count - 1 && _comparer.Compare(editItem, _array[index + 1]) > 0) {
            var destIndex = _array.AsSpan(index, _count - 1 - index).BinarySearch(editItem, _comparer);
            TraNumber.FlipNegative(ref destIndex);
            MoveTo(index, destIndex - 1);
        }
        else {
            // No Move
        }
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        TraNumber.ValidateSliceArgs(arrayIndex, _count, array.Length);
        AsSpan().CopyTo(array.AsSpan(arrayIndex, _count));
    }

    public Enumerator GetEnumerator() => new(this);

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    int IList<T>.IndexOf(T item) => Math.Max(BinarySearch(item), -1);
    void IList<T>.Insert(int index, T item) => ThrowHelper.ThrowInvalidOperationException("Cannot insert at specific position to SortedList");
    bool ICollection<T>.Contains(T item) => BinarySearch(item) >= 0;
    bool ICollection<T>.Remove(T item) => Remove(item) >= 0;

    public struct Enumerator : IEnumerator<T>
    {
        private readonly SortedList<T> _list;
        private int _index;
        private int _version;
        private T _current;

        internal Enumerator(SortedList<T> list)
        {
            _list = list;
            _index = 0;
            _version = _list._version;
            _current = default!;
        }

        public T Current => _current;

        object IEnumerator.Current => Current!;

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

        public void Reset() => _index = 0;
        public void Dispose() { }
    }
}
