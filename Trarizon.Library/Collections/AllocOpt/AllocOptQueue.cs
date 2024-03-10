using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Trarizon.Library.Collections.AllocOpt;
[CollectionBuilder(typeof(AllocOptCollectionBuilder), nameof(AllocOptCollectionBuilder.CreateQueue))]
public struct AllocOptQueue<T> : ICollection<T>, IReadOnlyCollection<T>
{
    private AllocOptList<T> _list;
    private int _head;
    private int _tail;

    public AllocOptQueue()
    {
        _list = [];
        _head = 0;
    }

    public AllocOptQueue(int capacity)
    {
        _list = new AllocOptList<T>(capacity);
        _head = 0;
    }

    #region Accessors

    public readonly bool IsEmpty => _head == _tail && _list.Count == 0;

    public readonly int Count
    {
        get {
            if (_tail > _head)
                return _tail - _head;

            // If empty, the underlying list will be cleared, so this will return 0;
            if (_tail == _head)
                return _list.Count;

            return _list.Count - (_head - _tail);
        }
    }

    public readonly int Capacity => _list.Capacity;

    public readonly T Peek() => _list[_head];

    public readonly bool TryPeek([MaybeNullWhen(false)] out T value)
    {
        if (IsEmpty) {
            value = default;
            return false;
        }
        value = Peek();
        return true;
    }

    public readonly T[] ToArray()
    {
        var count = Count;
        if (count == 0)
            return [];

        var res = new T[count];
        CopyTo(res, 0);
        return res;
    }

    public readonly T[] GetUnderlyingArray() => _list.GetUnderlyingArray();

    public readonly Enumerator GetEnumerator() => new(this);

    #endregion

    #region Builders

    public void Enqueue(T item)
    {
        // Never full-filled, just add item as list
        if (_list.Count != _list.Capacity) {
            _list.Add(item);
        }
        // Full, manually grow which copy items to new array in order, and add as list
        if (Count == Capacity) {
            Grow(Capacity + 1);
            _list.Add(item);
        }
        else {
            _list[_tail] = item;
        }
        Increment(ref _tail);
    }

    public void Dequeue()
    {
        if (IsEmpty)
            ThrowHelper.ThrowInvalidOperation("Queue is empty");

        Increment(ref _head);

        // Empty
        if (_head == _tail) {
            Clear();
        }
    }

    public bool TryDequeue([MaybeNullWhen(false)] out T item)
    {
        if (TryPeek(out item)) {
            Dequeue();
            return true;
        }
        else {
            return false;
        }
    }

    /// <summary>
    /// This method won't clear elements in underlying array.
    /// Use <see cref="ClearUnreferenced"/> if you need it.
    /// </summary>
    public void Clear()
    {
        _list.Clear();
        _head = _tail = 0;
    }

    public readonly void ClearUnreferenced()
    {
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            return;

        if (_head < _tail) {
            if (_head > 0)
                Array.Clear(GetUnderlyingArray(), 0, _head);
            if (_tail < Capacity)
                Array.Clear(GetUnderlyingArray(), _tail, Capacity - _tail);
        }
        else {
            if (_head != _tail) // Not full queue
                Array.Clear(GetUnderlyingArray(), _tail, _head - _tail);
            else if (_list.Count == 0) // Empty queue
                Array.Clear(GetUnderlyingArray());
            // Full queue, do nothing
        }
    }

    public void EnsureCapacity(int capacity)
    {
        if (capacity <= Capacity)
            return;
        Grow(capacity);
    }

    private void Grow(int expectedCapacity)
    {
        _list.GrowEmptyArray(expectedCapacity, out var oriArray);
        if (_tail > _head) {
            Array.Copy(oriArray, _head, GetUnderlyingArray(), 0, _tail - _head);
        }
        else {
            var length = oriArray.Length - _head;
            Array.Copy(oriArray, _head, GetUnderlyingArray(), 0, length);
            Array.Copy(oriArray, 0, GetUnderlyingArray(), length, _tail);
        }
        _head = 0;
        _tail = oriArray.Length;
    }

    private readonly void Increment(ref int index)
    {
        if (index + 1 == Capacity)
            index = 0;
        else
            index++;
    }

    #endregion

    #region Interface methods

    readonly bool ICollection<T>.IsReadOnly => false;

    public readonly bool Contains(T item)
    {
        if (_head < _tail)
            return Array.IndexOf(GetUnderlyingArray(), item, _head, _tail - _head) >= 0;

        return Array.IndexOf(GetUnderlyingArray(), item, 0, _tail) >= 0
            || Array.IndexOf(GetUnderlyingArray(), item, _head, Capacity - _head) >= 0;
    }
    public readonly void CopyTo(T[] array, int arrayIndex)
    {
        if (_tail > _head) {
            Array.Copy(GetUnderlyingArray(), _head, array, 0, _tail - _head);
        }
        else {
            var length = Capacity - _head;
            Array.Copy(GetUnderlyingArray(), _head, array, 0, length);
            Array.Copy(GetUnderlyingArray(), 0, array, length, _tail);
        }
    }

    void ICollection<T>.Add(T item) => Enqueue(item);
    readonly bool ICollection<T>.Remove(T item)
    {
        ThrowHelper.ThrowNotSupport("Cannot remove specific item from stack");
        return default;
    }

    readonly IEnumerator<T> IEnumerable<T>.GetEnumerator() => new Enumerator.Wrapper(this);
    readonly IEnumerator IEnumerable.GetEnumerator() => new Enumerator.Wrapper(this);

    #endregion

    public struct Enumerator
    {
        private readonly AllocOptList<T> _underlyingList;
        private readonly int _tail;
        private int _index;

        internal Enumerator(in AllocOptQueue<T> queue)
        {
            _underlyingList = queue._list;
            _tail = queue._tail;
            _index = ~queue._head;
        }

        public readonly T Current => _underlyingList[_index];

        public bool MoveNext()
        {
            if (_underlyingList.Count == 0)
                return false;

            // First
            if (_index < 0) {
                _index = ~_index;
                return true;
            }

            var index = _index + 1;
            if (index >= _underlyingList.Capacity) {
                index = 0;
            }

            // Last
            if (index == _tail) {
                return false;
            }

            _index = index;
            return true;
        }

        internal sealed class Wrapper(in AllocOptQueue<T> queue) : IEnumerator<T>
        {
            private Enumerator _enumerator = queue.GetEnumerator();
            private readonly int _head = queue._head;

            public T Current => _enumerator.Current;

            object? IEnumerator.Current => Current;

            public void Dispose() { }
            public bool MoveNext() => _enumerator.MoveNext();
            public void Reset() => _enumerator._index = ~_head;
        }
    }
}
