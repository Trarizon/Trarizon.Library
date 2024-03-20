using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Trarizon.Library.CodeAnalysis.MemberAccess;
using Trarizon.Library.Collections.StackAlloc;

namespace Trarizon.Library.Collections.AllocOpt;
[CollectionBuilder(typeof(AllocOptCollectionBuilder), nameof(AllocOptCollectionBuilder.CreateDeque))]
public struct AllocOptDeque<T> : ICollection<T>, IReadOnlyCollection<T>
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

    public AllocOptDeque()
    {
        _list = [];
        _head = 0;
    }

    public AllocOptDeque(int capacity)
    {
        _list = new AllocOptList<T>(capacity);
        _head = 0;
    }

    /// <remarks>
    /// This ctor wont create empty queue
    /// </remarks>
    [FriendAccess(typeof(AllocOptCollectionBuilder))]
    internal AllocOptDeque(T[] items, int head, int tail)
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

    internal readonly ref T AtRefOrNullRef(int index)
    {
        if (index < 0)
            return ref Unsafe.NullRef<T>();

        var arrIndex = index + _head;
        if (arrIndex > Capacity)
            arrIndex -= arrIndex;

        if (arrIndex >= _tail)
            return ref Unsafe.NullRef<T>();
        return ref _list.AtRefOrNullRef(index);
    }

    public readonly int Capacity => _list.Capacity;

    public readonly T PeekFirst() => _list[_head];

    public readonly ReadOnlyQueueSpan<T> PeekFirst(int count)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(count, Count);
        var head = _head + count;
        if (head > Capacity)
            head -= Capacity;
        return new(GetUnderlyingArray(), head, _tail);
    }

    public readonly bool TryPeekFirst([MaybeNullWhen(false)] out T item)
    {
        if (IsEmpty) {
            item = default;
            return false;
        }
        item = PeekFirst();
        return true;
    }

    public readonly bool TryPeekFirst(int count, out ReadOnlyQueueSpan<T> items)
    {
        if (count > Count) {
            items = default;
            return false;
        }
        var tail = _head + count;
        if (tail > Capacity)
            tail -= Capacity;
        items = new(GetUnderlyingArray(), _head, tail);
        return true;
    }

    public readonly T PeekLast()
    {
        var index = _tail;
        Decrement(ref index);
        return _list[index];
    }

    public readonly ReversedReadOnlyQueueSpan<T> PeekLast(int count)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(count, Count);
        var head = _tail - count;
        if (head < 0)
            head += Capacity;
        return new ReadOnlyQueueSpan<T>(GetUnderlyingArray(), head, _tail).Reverse();
    }

    public readonly bool TryPeekLast([MaybeNullWhen(false)] out T item)
    {
        if (IsEmpty) {
            item = default;
            return false;
        }
        item = PeekLast();
        return true;
    }

    public readonly bool TryPeekLast(int count, out ReversedReadOnlyQueueSpan<T> items)
    {
        if (count > Count) {
            items = default;
            return false;
        }
        var head = _tail - count;
        if (head < 0)
            head += Capacity;
        items = new ReadOnlyQueueSpan<T>(GetUnderlyingArray(), head, _tail).Reverse();
        return true;
    }

    public readonly ReadOnlyQueueSpan<T> AsSpan() => new(GetUnderlyingArray(), _head, _tail);

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

    public void EnqueueFirst(T item)
    {
        // When empty, we treat as EnqueueLast which is cheaper
        if (IsEmpty) {
            EnqueueLast(item);
            return;
        }

        // |  ----  |
        if (_tail > _head) {
            Decrement(ref _head);
            if (_head == _tail) // Count == Capacity
                goto RequiresGrow;

            if (_head >= _list.Count) { // Underlying list is not full
                _list.SetCount(Capacity);
            }
            _list[_head] = item;
        }
        // |--  --|
        else {
            Debug.Assert(_head > 0);
            _head--;
            if (_head == _tail)
                goto RequiresGrow;

            _list[_head] = item;
        }

        return;

    RequiresGrow:
        GrowForEnqueueFirst(1);
        Debug.Assert(_head == 1);
        _head = 0;
        _list[0] = item;
    }

    public void EnqueueLast(T item)
    {
        // Never full-filled, just add item as list
        if (_list.Count != _list.Capacity) {
            _list.Add(item);
        }
        // Full, manually grow which copy items to new array in order, and add as list
        else if (Count == Capacity) {
            Grow(Capacity + 1);
            _list.Add(item);
        }
        else {
            _list[_tail] = item;
        }

        Increment(ref _tail);
    }

    public void EnqueueRangeLast<TEnumerable>(TEnumerable collection) where TEnumerable : IEnumerable<T>
    {
        if (collection is ICollection<T> c) {
            EnqueueCollectionLast(c);
            return;
        }

        foreach (var item in collection) {
            EnqueueLast(item);
        }
    }

    public void EnqueueRangeFirst<TEnumerable>(TEnumerable collection) where TEnumerable : IEnumerable<T>
    {
        if (collection is ICollection<T> c) {
            EnqueueCollectionFirst(c);
            return;
        }

        foreach (var item in collection) {
            EnqueueFirst(item);
        }
    }

    public void EnqueueCollectionFirst<TCollection>(TCollection collection) where TCollection : ICollection<T>
    {
        int count = collection.Count;
        if (count <= 0)
            return;

        if (collection is T[] array) {
            EnqueueRangeFirst(array.AsSpan());
            return;
        }
        if (collection is List<T> list) {
            EnqueueRangeFirst(CollectionsMarshal.AsSpan(list));
            return;
        }

        var newCount = Count + count;
        // Grow required
        if (newCount > Capacity) {
            GrowForEnqueueFirst(count);
            collection.CopyTo(GetUnderlyingArray(), 0);
        }
        // | <==----|
        else if (_head >= count) {
            _head -= count;
            collection.CopyTo(GetUnderlyingArray(), _head);
        }
        // |==-- <==|
        else {
            _head = _head - count + Capacity;
            _list.SetCount(Capacity);
            var it = _head;
            foreach (var item in collection) {
                _list[it] = item;
                Increment(ref it);
            }
        }
    }

    public void EnqueueCollectionLast<TCollection>(TCollection collection) where TCollection : ICollection<T>
    {
        int count = collection.Count;
        if (count <= 0)
            return;

        if (collection is T[] array) {
            EnqueueRangeLast(array.AsSpan());
            return;
        }
        if (collection is List<T> list) {
            EnqueueRangeLast(CollectionsMarshal.AsSpan(list));
            return;
        }

        var newCount = Count + count;
        // Grow required
        if (newCount > Capacity) {
            Grow(newCount); // this will set _tail to _list.Count;
            goto AppendTail;
        }
        // |--->==>  |
        else if (_tail + count < Capacity) {
            goto AppendTail;
        }
        // |==>  -->==|
        else {
            foreach (var item in collection)
                EnqueueLast(item);
            return;
        }


#pragma warning disable CS0162
        Debug.Assert(false);
#pragma warning restore CS0162

    AppendTail:
        Debug.Assert(_tail + count <= Capacity);
        var newTail = _tail + count;
        _list.SetCount(newTail);
        collection.CopyTo(GetUnderlyingArray(), _tail);
        _tail = newTail;
        return;
    }

    public void EnqueueRangeFirst(ReadOnlySpan<T> items)
    {
        if (items.Length == 0)
            return;

        var count = items.Length;
        var newCount = Count + count;
        // Grow required
        if (newCount > Capacity) {
            GrowForEnqueueFirst(count);
            items.CopyTo(GetUnderlyingArray());
        }
        // | <==----|
        else if (_head >= count) {
            _head -= count;
            items.CopyTo(GetUnderlyingArray().AsSpan(_head, items.Length));
        }
        // |==-- <==|
        else {
            var newHead = _head - count + Capacity;
            _list.SetCount(Capacity);
            items[^_head..].CopyTo(GetUnderlyingArray());
            items[.._head].CopyTo(GetUnderlyingArray().AsSpan(newHead, _head));
            _head = newHead;
        }
    }

    public void EnqueueRangeLast(ReadOnlySpan<T> items)
    {
        if (items.Length == 0)
            return;

        // Grow required
        var newCount = Count + items.Length;
        if (newCount > Capacity) {
            Grow(newCount); // this will set _tail to _list.Count;
            goto AppendTail;
        }
        // |---==>  |
        else if (_tail + items.Length < Capacity) {
            goto AppendTail;
        }
        // |=>  --==|
        else {
            var mid = Capacity - _tail;
            items[..mid].CopyTo(GetUnderlyingArray().AsSpan(_tail, mid));
            _tail = items.Length - mid;
            items[mid.._tail].CopyTo(GetUnderlyingArray());
            return;
        }

#pragma warning disable CS0162
        Debug.Assert(false);
#pragma warning restore CS0162

    AppendTail:
        Debug.Assert(_tail + items.Length <= Capacity);
        var newTail = _tail + items.Length;
        _list.SetCount(newTail);
        items.CopyTo(GetUnderlyingArray().AsSpan(_tail, items.Length));
        _tail = newTail;
        return;
    }

    public T DequeueFirst()
    {
        if (IsEmpty)
            ThrowHelper.ThrowInvalidOperation("Deque is empty");

        var rtn = _list[_head];
        Increment(ref _head);

        // Empty
        if (_head == _tail) {
            Clear();
        }

        return rtn;
    }

    public T DequeueLast()
    {
        if (IsEmpty)
            ThrowHelper.ThrowInvalidOperation("Deque is empty");

        Decrement(ref _tail);
        var rtn = _list[_tail];

        // Empty
        if (_head == _tail) {
            Clear();
        }

        return rtn;
    }

    /// <summary>
    /// This wont throw exception if stack is empty
    /// </summary>
    public void DequeueFirst(int count)
    {
        if (count <= 0)
            return;
        if (count >= Count) {
            Clear();
            return;
        }

        // |  ----  |
        var newHead = _head + count;
        if (_tail > _head) {
            _head = newHead;
            Debug.Assert(_tail > _head);
        }
        else if (newHead < Capacity) {
            _head = newHead;
        }
        else {
            _head = newHead - Capacity;
        }
    }

    /// <summary>
    /// This wont throw exception if stack is empty
    /// </summary>
    public void DequeueLast(int count)
    {
        if (count <= 0)
            return;
        if (count >= Count) {
            Clear();
            return;
        }


        var newTail = _tail - count;
        if (_tail > _head) {
            _tail = newTail;
        }
        else if (newTail >= 0) {
            _tail = newTail;
        }
        else {
            _tail = newTail + Capacity;
        }
    }

    public bool TryDequeueFirst([MaybeNullWhen(false)] out T item)
    {
        if (TryPeekFirst(out item)) {
            DequeueFirst();
            return true;
        }
        else {
            return false;
        }
    }

    public bool TryDequeueLast([MaybeNullWhen(false)] out T item)
    {
        if (TryPeekLast(out item)) {
            DequeueLast();
            return true;
        }
        else {
            return false;
        }
    }

    /// <summary>
    /// This method won't clear elements in underlying array.
    /// Use <see cref="FreeUnreferenced"/> if you need it.
    /// </summary>
    public void Clear()
    {
        _list.Clear();
        _head = _tail = 0;
    }

    public readonly void FreeUnreferenced()
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
        var count = Count; // cache old count
        _list.SetCountNonMove(expectedCapacity, out var oriArray);
        MoveItems(oriArray, 0);
        _head = 0;
        _tail = count;
    }

    private void GrowForEnqueueFirst(int enqueueCount)
    {
        var count = Count;
        _list.SetCountNonMove(count + enqueueCount, out var oriArray);
        MoveItems(oriArray, enqueueCount);
        _head = enqueueCount;
        _tail = count + enqueueCount;
    }

    /// <summary>
    /// before reset _tail and _head
    /// </summary>
    private readonly void MoveItems(T[] originalArray, int offsetInNewArray)
    {
        if (_tail > _head) {
            var len = _tail - _head;
            originalArray.AsSpan(_head, len).CopyTo(GetUnderlyingArray().AsSpan(offsetInNewArray, len));
        }
        else {
            var mid = originalArray.Length - _head;
            originalArray.AsSpan(_head, mid).CopyTo(GetUnderlyingArray().AsSpan(offsetInNewArray, mid));
            originalArray.AsSpan(0, _tail).CopyTo(GetUnderlyingArray().AsSpan(offsetInNewArray + mid, _tail));
        }
    }

    private readonly void Increment(ref int index)
    {
        if (index + 1 == Capacity)
            index = 0;
        else
            index++;
    }

    private readonly void Decrement(ref int index)
    {
        if (index == 0)
            index = Capacity - 1;
        else
            index--;
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

    // Mirrored in AOQueue, Deque
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

    readonly IEnumerator<T> IEnumerable<T>.GetEnumerator() => new Enumerator.Wrapper(this);
    readonly IEnumerator IEnumerable.GetEnumerator() => new Enumerator.Wrapper(this);

    #endregion

    public struct Enumerator
    {
        // As value type, change indexes in _queue will not affect
        // the original queue, so we use _queue._head as moving index
        // of this Enumerator
        private AllocOptDeque<T> _queue;
        private T? _current;

        internal Enumerator(in AllocOptDeque<T> queue)
        {
            _queue = queue;
        }

        public readonly T Current => _current!;

        public bool MoveNext() => _queue.TryDequeueFirst(out _current!);

        internal sealed class Wrapper(in AllocOptDeque<T> queue) : IEnumerator<T>
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
