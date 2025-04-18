using CommunityToolkit.Diagnostics;
using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Trarizon.Library.Collections.StackAlloc;

namespace Trarizon.Library.Collections.Generic;
[CollectionBuilder(typeof(CollectionBuilders), nameof(CollectionBuilders.CreateDeque))]
public class Deque<T> : ICollection<T>, IReadOnlyCollection<T>
{
    private T[] _array;
    private int _count;
    private int _head;
    private int _tail;
    private int _version;

    public Deque()
    {
        _array = [];
    }

    public Deque(int capacity)
    {
        Guard.IsGreaterThanOrEqualTo(capacity, 0);
        _array = new T[capacity];
    }

    #region Accessors

    public int Count => _count;

    public int Capacity => _array.Length;

    public T PeekFirst() => _array[_head];

    public bool TryPeekFirst([MaybeNullWhen(false)] out T item)
    {
        if (_count == 0) {
            item = default;
            return false;
        }
        else {
            item = PeekFirst();
            return true;
        }
    }

    public T PeekLast() => _array[Decrement(_tail)];

    public bool TryPeekLast([MaybeNullWhen(false)] out T item)
    {
        if (_count == 0) {
            item = default;
            return false;
        }
        else {
            item = PeekLast();
            return true;
        }
    }

    public ReadOnlyConcatSpan<T> AsSpan() => AsMutableSpan();

    private ConcatSpan<T> AsMutableSpan()
    {
        if (_count == 0)
            return default;

        // | --- |
        if (_head < _tail)
            return new(_array.AsSpan(_head.._tail), default);
        // |-- --|
        else
            return new(_array.AsSpan(_head), _array.AsSpan(0, _tail));
    }

    public T[] ToArray()
    {
        if (_count == 0)
            return [];

        var res = new T[_count];
        CopyTo(res, 0);
        return res;
    }

    public bool Contains(T item)
    {
        if (_count == 0)
            return false;

        if (_head < _tail)
            return Array.IndexOf(_array, item, _head, _tail - _head) >= 0;
        else
            return Array.IndexOf(_array, item, _head, Capacity - _head) >= 0
                || Array.IndexOf(_array, item, 0, _head) >= 0;
    }

    public Enumerator GetEnumerator() => new(this);

    #endregion

    #region Modifiers

    public void EnqueueFirst(T item)
    {
        if (_count == Capacity) {
            var capacity = Capacity;
            GrowAndCopy(capacity + 1, 1);
            _head = 0;
            _tail = Increment(capacity);
        }
        else {
            _head = Decrement(_head);
        }
        _array[_head] = item;
        _count++;
        _version++;
    }

    public void EnqueueLast(T item)
    {
        if (_count == Capacity) {
            var capacity = Capacity;
            GrowAndCopy(capacity + 1, 0);
            _head = 0;
            _tail = capacity;
        }
        _array[_tail] = item;
        _tail = Increment(_tail);
        _count++;
        _version++;
    }

    public void EnqueueRangeFirst(IEnumerable<T> collection)
    {
        if (collection is ICollection<T> col) {
            EnqueueRangeCollection(col);
            _version++;
            return;
        }


        foreach (var item in collection.IterReverse()) {
            EnqueueFirst(item);
        }

        void EnqueueRangeCollection(ICollection<T> col)
        {
            var count = col.Count;
            if (count <= 0)
                return;

            var newCount = _count + count;
            if (newCount > Capacity) {
                GrowAndCopy(newCount, count);
                col.CopyTo(_array, 0);
                _head = 0;
                _tail = newCount == Capacity ? 0 : newCount;
            }
            // |-- <==--|
            else if (_head >= count) {
                _head -= count;
                col.CopyTo(_array, _head);
            }
            // |=---- <=|
            else {
                _head = _head - count + Capacity;
                var seperator = Capacity - _head;
                if (col.TryGetSpan(out var span)) {
                    span[..seperator].CopyTo(_array.AsSpan(_head..));
                    span[seperator..].CopyTo(_array);
                }
                else {
                    using var enumerator = col.GetEnumerator();
                    for (int i = 0; i < seperator; i++) {
                        var hasNext = enumerator.MoveNext();
                        Debug.Assert(hasNext);
                        _array[_head + i] = enumerator.Current;
                    }
                    for (int i = seperator; i < count; i++) {
                        var hasNext = enumerator.MoveNext();
                        Debug.Assert(hasNext);
                        _array[i - seperator] = enumerator.Current;
                    }
                }
            }
            _count = newCount;
        }
    }

    public void EnqueueRangeFirst(ReadOnlySpan<T> span)
    {
        var count = span.Length;
        if (count <= 0)
            return;

        var newCount = _count + count;
        if (newCount > Capacity) {
            GrowAndCopy(newCount, count);
            span.CopyTo(_array);
            _head = 0;
            _tail = newCount == Capacity ? 0 : newCount;
        }
        // |-- <==--|
        else if (_head >= count) {
            _head -= count;
            span.CopyTo(_array.AsSpan(_head));
        }
        // |=---- <=|
        else {
            _head = _head - count + Capacity;
            var seperator = Capacity - _head;
            span[..seperator].CopyTo(_array.AsSpan(_head..));
            span[seperator..].CopyTo(_array);
        }
        _count = newCount;
        _version++;
    }

    public void EnqueueRangeLast(IEnumerable<T> collection)
    {
        if (collection is ICollection<T> col) {
            EnqueueRangeCollection(col);
            _version++;
            return;
        }

        foreach (var item in collection) {
            EnqueueLast(item);
        }

        void EnqueueRangeCollection(ICollection<T> col)
        {
            var count = col.Count;
            if (count <= 0)
                return;

            var oldCount = _count;
            var newCount = _count + count;
            if (newCount > Capacity) {
                GrowAndCopy(newCount, 0);
                col.CopyTo(_array, oldCount);
                _head = 0;
                _tail = newCount == Capacity ? 0 : newCount;
            }
            // |--==> --|
            else if (_tail + count <= Capacity) {
                col.CopyTo(_array, _tail);
                _tail += count;
            }
            // |=> ----=|
            else {
                var seperator = Capacity - _tail;
                if (col.TryGetSpan(out var span)) {
                    span[..seperator].CopyTo(_array.AsSpan(_tail..));
                    span[seperator..].CopyTo(_array);
                }
                else {
                    using var enumerator = col.GetEnumerator();
                    for (int i = 0; i < seperator; i++) {
                        var next = enumerator.MoveNext();
                        Debug.Assert(next == true);
                        _array[_tail + i] = enumerator.Current;
                    }
                    for (int i = seperator; i < count; i++) {
                        var next = enumerator.MoveNext();
                        Debug.Assert(next == true);
                        _array[i - seperator] = enumerator.Current;
                    }
                }
                _tail = _tail + count - Capacity;
            }
            _count = newCount;
        }
    }

    public void EnqueueRangeLast(ReadOnlySpan<T> span)
    {
        var count = span.Length;
        if (count <= 0)
            return;

        var oldCount = _count;
        var newCount = _count + count;
        if (newCount > Capacity) {
            GrowAndCopy(newCount, 0);
            span.CopyTo(_array.AsSpan(oldCount));
            _head = 0;
            _tail = newCount == Capacity ? 0 : newCount;
        }
        // |--==> --|
        else if (_tail + count <= Capacity) {
            span.CopyTo(_array.AsSpan(_tail));
            _tail += count;
        }
        // |=> ----=|
        else {
            var seperator = Capacity - _tail;
            span[..seperator].CopyTo(_array.AsSpan(_tail..));
            span[seperator..].CopyTo(_array);
            _tail = _tail + count - Capacity;
        }
        _count = newCount;
        _version++;
    }

    public T DequeueFirst()
    {
        if (!TryDequeueFirst(out var res)) {
            TraThrow.NoElement();
        }
        return res;
    }

    public T DequeueLast()
    {
        if (!TryDequeueLast(out var res)) {
            TraThrow.NoElement();
        }
        return res;
    }

    public bool TryDequeueFirst([MaybeNullWhen(false)] out T item)
    {
        if (_count == 0) {
            item = default;
            return false;
        }

        item = _array[_head];

        ArrayGrowHelper.FreeManaged(_array, _head);
        _head = Increment(_head);
        _count--;
        _version++;
        return true;
    }

    public bool TryDequeueLast([MaybeNullWhen(false)] out T item)
    {
        if (_count == 0) {
            item = default;
            return false;
        }

        _tail = Decrement(_tail);
        item = _array[_tail];

        ArrayGrowHelper.FreeManaged(_array, _tail);
        _count--;
        _version++;
        return true;
    }

    public void Clear()
    {
        if (_count == 0)
            return;
        ArrayGrowHelper.FreeManaged(AsMutableSpan());
        _head = _tail = 0;
        _count = 0;
        _version++;
    }

    #endregion

    public int EnsureCapacity(int capacity)
    {
        Guard.IsGreaterThanOrEqualTo(capacity, 0);
        if (_array.Length < capacity) {
            GrowAndCopy(capacity, 0);
        }
        return _array.Length;
    }

    public void TrimExcess()
    {
        int threshold = (int)(_array.Length * 0.9);
        if (_count < threshold) {
            _array = ToArray();
        }
    }

    public void TrimExcess(int capacity)
    {
        Guard.IsGreaterThanOrEqualTo(capacity, _count);
        if (capacity == _array.Length)
            return;

        var newArray = new T[capacity];
        CopyTo(newArray, 0);
        _array = newArray;
    }

    public void CopyTo(T[] array, int arrayIndex)
        => AsSpan().CopyTo(array.AsSpan(arrayIndex));

    private void GrowAndCopy(int expectedCapacity, int copyToIndex)
    {
        Debug.Assert(expectedCapacity > Capacity);
        var span = AsSpan();
        ArrayGrowHelper.GrowNonMove(ref _array, expectedCapacity);
        span.CopyTo(_array.AsSpan(copyToIndex, span.Length));
    }

    [DebuggerStepThrough]
    private int Decrement(int index) => index == 0 ? Capacity - 1 : index - 1;

    [DebuggerStepThrough]
    private int Increment(int index)
    {
        var res = index + 1;
        if (res >= Capacity)
            return res - Capacity;
        else
            return res;
    }

    bool ICollection<T>.IsReadOnly => false;
    void ICollection<T>.Add(T item) => EnqueueLast(item);
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
    bool ICollection<T>.Remove(T item) => false;
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public struct Enumerator : IEnumerator<T>
    {
        private readonly Deque<T> _deque;
        private readonly int _version;
        private int _index;
        private T? _current;

        internal Enumerator(Deque<T> deque)
        {
            _deque = deque;
            _version = _deque._version;
            _index = _deque.Count == 0 ? -1 : _deque._head;
        }

        public readonly T Current => _current!;

#nullable disable
        readonly object IEnumerator.Current => Current;
#nullable restore

        public bool MoveNext()
        {
            CheckVersion();

            if (_index < 0) {
                _current = default;
                return false;
            }

            // | -- |
            if (_deque._head < _deque._tail) {
                _current = _deque._array[_index];
                if (_index + 1 == _deque._tail)
                    _index = -1;
                else
                    _index++;
                return true;
            }
            // |- -|
            else {
                _current = _deque._array[_index];
                var next = _deque.Increment(_index);
                if (next == _deque._tail)
                    _index = -1;
                else
                    _index = next;
                return true;
            }
        }

        void IEnumerator.Reset()
        {
            CheckVersion();
            _index = _deque.Count == 0 ? -1 : _deque._head;
        }

        private readonly void CheckVersion()
        {
            if (_version != _deque._version)
                TraThrow.CollectionModified();
        }

        public readonly void Dispose() { }
    }
}
