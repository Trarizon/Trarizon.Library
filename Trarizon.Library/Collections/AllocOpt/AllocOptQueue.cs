using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Trarizon.Library.CodeAnalysis.MemberAccess;
using Trarizon.Library.Collections.StackAlloc;

namespace Trarizon.Library.Collections.AllocOpt;
[CollectionBuilder(typeof(AllocOptCollectionBuilder), nameof(AllocOptCollectionBuilder.CreateQueue))]
public struct AllocOptQueue<T> : ICollection<T>, IReadOnlyCollection<T>
{
    // |  -------  |
    //    ^head  ^tail
    // when _list.Count == _tail, parts after _tail are invalid, and _list.Add means same as Enqueue
    // when _head == _tail, queue is empty, always reset _head, _tail, _list
    // 
    // |---      ---|
    //     ^tail ^head
    // when _head == _tail, queue is full
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

    /// <remarks>
    /// This ctor wont create empty queue
    /// </remarks>
    [FriendAccess(typeof(AllocOptCollectionBuilder))]
    internal AllocOptQueue(T[] items, int head, int tail)
    {
        // when tail == head, means collection full
        if (tail <= head) {
            _list = AllocOptCollectionBuilder.AsList(items, items.Length);
        }
        else {
            _list = AllocOptCollectionBuilder.AsList(items, tail);
        }
        _head = head;
        _tail = tail;
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

    public readonly bool TryPeek([MaybeNullWhen(false)] out T item)
    {
        if (IsEmpty) {
            item = default;
            return false;
        }
        item = Peek();
        return true;
    }

    public readonly QueueSpan<T> AsSpan() => new(GetUnderlyingArray(), _head, _tail);

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

    public void EnqueueRange<TEnumerable>(TEnumerable collection) where TEnumerable : IEnumerable<T>
    {
        if (collection is ICollection<T> c) {
            EnqueueCollection(c);
            return;
        }

        foreach (var item in collection) {
            Enqueue(item);
        }
    }

    public void EnqueueCollection<TCollection>(TCollection collection) where TCollection : ICollection<T>
    {
        int count = collection.Count;
        if (count <= 0)
            return;

        if (collection is T[] array) {
            EnqueueRange(array.AsSpan());
        }
        else if (collection is List<T> list) {
            EnqueueRange(CollectionsMarshal.AsSpan(list));
        }

        var oldCount = Count;
        var newCount = oldCount + count;
        // Grow required
        if (newCount > Capacity) {
            Grow(newCount);
            goto InnerListAdd;
        }
        // Underlying array never full-filled
        else if (_tail == _list.Count) {
            // Directly add
            if (_tail + count < Capacity)
                goto InnerListAdd;
            else
                goto Enumerate;
        }
        // |  -->  |
        else if (_tail > _head) {
            // |  -->==>  |
            if (_tail + count < Capacity)
                goto AppendTail;
            // |==>  -->==|
            else
                goto Enumerate;
        }
        // |-->  --|
        else {
            goto AppendTail;
        }

#pragma warning disable CS0162
        Debug.Assert(false);
#pragma warning restore CS0162

    InnerListAdd:
        _list.OverwriteCollectionNonGrow(oldCount, collection);
        _tail = newCount;
        return;

    Enumerate:
        foreach (var item in collection)
            Enqueue(item);
        return;

    AppendTail:
        _list.OverwriteCollectionNonGrow(_tail, collection);
        _tail += count;
        return;
    }

    public void EnqueueRange(ReadOnlySpan<T> items)
    {
        if (items.Length == 0)
            return;

        // Grow required
        var oldCount = Count;
        var newCount = oldCount + items.Length;
        if (newCount > Capacity) {
            Grow(newCount);
            goto InnerListAdd;
        }
        // Underlying array never full-filled
        else if (_tail == _list.Count) {
            // |  -->==>  |
            // Can directly add
            if (_tail + items.Length < Capacity) {
                goto InnerListAdd;
            }
            // |==>  -->==|
            // Add to full and copy rest part to first
            else
                goto TwoPartsAdd;
        }
        // |  -->  |
        else if (_tail > _head) {
            // |  -->==>  |
            if (_tail + items.Length < Capacity)
                goto AppendTail;
            // |==>  -->==|
            else
                goto TwoPartsAdd;
        }
        // |-->  --|
        else {
            goto AppendTail;
        }

#pragma warning disable CS0162
        Debug.Assert(false);
#pragma warning restore CS0162

    InnerListAdd:
        _list.OverwriteRangeNonGrow(oldCount, items);
        _tail = newCount;
        return;

    TwoPartsAdd:
        var mid = Capacity - _tail;
        _list.OverwriteRangeNonGrow(_tail, items[..mid]);
        _tail = items.Length - mid;
        _list.OverwriteRangeNonGrow(0, items.Slice(mid, _tail));
        return;

    AppendTail:
        _list.OverwriteRangeNonGrow(_tail, items);
        _tail += items.Length;
        return;
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

    /// <summary>
    /// This wont throw exception if stack is empty
    /// </summary>
    public void Dequeue(int count)
    {
        if (count <= 0)
            return;

        if (_tail > _head) {
            var newHead = _head + count;
            if (_tail > newHead) {
                _head = newHead;
                return;
            }
            else {
                Clear();
                return;
            }
        }
        else {
            var newHead = _head + count;
            if (newHead < Capacity) {
                _head = newHead;
                return;
            }

            newHead -= Capacity;
            if (_tail > newHead) {
                _head = newHead;
                return;
            }
            else {
                Clear();
                return;
            }
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
        var count = Count;
        _list.GrowEmptyArray(expectedCapacity, out var oriArray);
        if (_tail > _head) {
            _list.OverwriteRangeNonGrow(0, oriArray.AsSpan(_head, _tail - _head));
        }
        else {
            var mid = oriArray.Length - _head;
            _list.OverwriteRangeNonGrow(0, oriArray.AsSpan(_head, mid));
            _list.OverwriteRangeNonGrow(mid, oriArray.AsSpan(0, _tail));
        }
        _head = 0;
        _tail = count;
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
        // As value type, change indexes in _queue will not affect
        // the original queue, so we use _queue._head as moving index
        // of this Enumerator
        private AllocOptQueue<T> _queue;
        private T? _current;

        internal Enumerator(in AllocOptQueue<T> queue)
        {
            _queue = queue;
        }

        public readonly T Current => _current!;

        public bool MoveNext() => _queue.TryDequeue(out _current!);

        internal sealed class Wrapper(in AllocOptQueue<T> queue) : IEnumerator<T>
        {
            private Enumerator _enumerator = queue.GetEnumerator();
            private readonly int _head = queue._head;

            public T Current => _enumerator.Current;

            object? IEnumerator.Current => Current;

            public void Dispose() { }
            public bool MoveNext() => _enumerator.MoveNext();
            public void Reset() => _enumerator._queue._head = _head;
        }
    }
}
