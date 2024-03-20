using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Trarizon.Library.Collections.AllocOpt;

namespace Trarizon.Library.Collections.Generic;
public class Deque<T> : ICollection<T>, IReadOnlyCollection<T>
{
    private AllocOptDeque<T> _deque;
    private int _version;

    public Deque() => _deque = [];

    public Deque(int capacity) => _deque = new AllocOptDeque<T>(capacity);

    #region Accessors

    public bool IsEmpty => _deque.IsEmpty;

    public int Count => _deque.Count;

    public int Capacity => _deque.Capacity;

    public T PeekFirst() => _deque.PeekFirst();

    public bool TryPeekFirst([MaybeNullWhen(false)] out T item) => _deque.TryPeekFirst(out item);

    public T PeekLast() => _deque.PeekLast();

    public bool TryPeekLast([MaybeNullWhen(false)] out T item) => _deque.TryPeekLast(out item);

    [SuppressMessage("Style", "IDE0305:简化集合初始化", Justification = "<挂起>")]
    public T[] ToArray() => _deque.ToArray();

    public Enumerator GetEnumerator() => new(this);

    #endregion

    #region Builders

    public void EnqueueFirst(T item)
    {
        _deque.EnqueueFirst(item);
        _version++;
    }

    public void EnqueueLast(T item)
    {
        _deque.EnqueueLast(item);
        _version++;
    }

    public void EnqueueRangeFirst(IEnumerable<T> collection)
    {
        _deque.EnqueueRangeFirst(collection);
        _version++;
    }

    public void EnqueueRangeLast(IEnumerable<T> collection)
    {
        _deque.EnqueueRangeLast(collection);
        _version++;
    }

    public void EnqueueRangeFirst(ReadOnlySpan<T> items)
    {
        _deque.EnqueueRangeFirst(items);
        _version++;
    }

    public void EnqueueRangeLast(ReadOnlySpan<T> items)
    {
        _deque.EnqueueRangeLast(items);
        _version++;
    }

    public T DequeueFirst()
    {
        var res = DequeueFirst();
        _version++;
        return res;
    }

    public T DequeueLast()
    {
        var res = _deque.DequeueLast();
        _version++;
        return res;
    }

    /// <summary>
    /// This wont throw exception if stack is empty
    /// </summary>
    public void DequeueFirst(int count)
    {
        _deque.DequeueFirst(count);
        _version++;
    }

    /// <summary>
    /// This wont throw exception if stack is empty
    /// </summary>
    public void DequeueLast(int count)
    {
        _deque.DequeueLast(count);
        _version++;
    }

    public bool TryDequeueFirst([MaybeNullWhen(false)] out T item)
    {
        if (_deque.TryDequeueFirst(out item)) {
            _version++;
            return true;
        }
        return false;
    }

    public bool TryDequeueLast([MaybeNullWhen(false)] out T item)
    {
        if (_deque.TryDequeueLast(out item)) {
            _version++;
            return true;
        }
        return false;
    }

    /// <summary>
    /// This method won't clear elements in underlying array.
    /// Use <see cref="FreeUnreferenced"/> if you need it.
    /// </summary>
    public void Clear()
    {
        _deque.Clear();
        _deque.FreeUnreferenced();
        _version++;
    }

    public int EnsureCapacity(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);

        _deque.EnsureCapacity(capacity);
        return _deque.Capacity;
    }

    public void TrimExcess()
    {
        int threshold = (int)(Capacity * 0.9);
        if (Count < threshold) {
            var newArray = _deque.ToArray();
            _deque = AllocOptCollectionBuilder.AsDeque(newArray, 0, newArray.Length);
            _version++;
        }
    }

    public void TrimExcess(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
        ArgumentOutOfRangeException.ThrowIfLessThan(capacity, Count);
        if (capacity == Count)
            return;

        var newArray = new T[capacity];
        _deque.CopyTo(newArray, 0);
        _deque = AllocOptCollectionBuilder.AsDeque(newArray, 0, Count);
        _version++;
    }

    #endregion

    #region Interface methods

    bool ICollection<T>.IsReadOnly => false;

    public bool Contains(T item) => _deque.Contains(item);
    public void CopyTo(T[] array, int arrayIndex) => _deque.CopyTo(array, arrayIndex);

    void ICollection<T>.Add(T item) => EnqueueLast(item);
    bool ICollection<T>.Remove(T item)
    {
        if (TryPeekFirst(out var val) && EqualityComparer<T>.Default.Equals(val, item)) {
            DequeueFirst(1);
            return true;
        }
        else if (TryPeekLast(out val) && EqualityComparer<T>.Default.Equals(val, item)) {
            DequeueLast(1);
            return true;
        }
        return false;
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion

    public struct Enumerator : IEnumerator<T>
    {
        private readonly Deque<T> _deque;
        private readonly int _version;
        private int _index;
        private T _current;

        internal Enumerator(Deque<T> deque)
        {
            _deque = deque;
            _version = deque._version;
            _index = -1;
            _current = default!;
        }

        public readonly T Current => _current;

        readonly object? IEnumerator.Current => Current;

        public void Dispose()
        {
            _index = -2;
            _current = default!;
        }

        public bool MoveNext()
        {
            if (_version != _deque._version)
                ThrowHelper.ThrowInvalidOperation("Collection has been changed after enumerator created.");

            if (_index == -2)
                return false;

            var index = _index + 1;

            ref var val = ref _deque._deque.AtRefOrNullRef(index);
            if (Unsafe.IsNullRef(in val)) {
                _index = 2;
                _current = default!;
                return false;
            }
            _index = index;
            _current = val;
            return true;
        }

        public void Reset()
        {
            if (_version != _deque._version)
                ThrowHelper.ThrowInvalidOperation("Collection has been changed after enumerator created.");
            _index = -1;
        }
    }
}
