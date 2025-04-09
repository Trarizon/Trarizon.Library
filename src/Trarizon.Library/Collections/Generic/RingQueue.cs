using CommunityToolkit.Diagnostics;
using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Trarizon.Library.Collections.StackAlloc;

namespace Trarizon.Library.Collections.Generic;
public enum RingQueueFullBehaviour
{
    Overwrite = 0,
    Throw,
}

public class RingQueue<T> : ICollection<T>, IReadOnlyCollection<T>
{
    private T[] _array;
    private int _count;
    private int _head;
    private int _tail;
    private readonly RingQueueFullBehaviour _fullBehaviour;
    private readonly int _maxCount;
    private int _version;

    public RingQueue(int maxCount, RingQueueFullBehaviour fullBehaviour = RingQueueFullBehaviour.Overwrite)
    {
        Guard.IsGreaterThan(maxCount, 0);
        _array = [];
        _count = _head = _tail = 0;
        _fullBehaviour = fullBehaviour;
        _maxCount = maxCount;
    }

    public RingQueue(int maxCount, int initialCapacity, RingQueueFullBehaviour fullBehaviour = RingQueueFullBehaviour.Overwrite)
    {
        Guard.IsGreaterThan(maxCount, 0);
        Guard.IsGreaterThanOrEqualTo(initialCapacity, 0);
        _array = initialCapacity == 0 ? [] : new T[Math.Min(maxCount, initialCapacity)];
        _count = _head = _tail = 0;
        _fullBehaviour = fullBehaviour;
        _maxCount = maxCount;
    }

    public int Count => _count;

    public int MaxCount => _maxCount;

    public bool IsFull => _count == _maxCount;

    public ReadOnlyConcatSpan<T> AsSpan() => AsMutableSpan();

    private ConcatSpan<T> AsMutableSpan()
    {
        if (_count == 0)
            return default;

        // |  ---  |
        if (_head < _tail)
            return new(_array.AsSpan(_head.._tail), default);
        // |--  --|
        else
            return new(_array.AsSpan(_head), _array.AsSpan(0, _tail));
    }

    public T PeekFirst()
    {
        if (!TryPeekFirst(out var item))
            TraThrow.NoElement();
        return item;
    }

    public T PeekLast()
    {
        if (!TryPeekLast(out var item))
            TraThrow.NoElement();
        return item;
    }

    public bool TryPeekFirst([MaybeNullWhen(false)] out T item)
    {
        if (_count > 0) {
            item = _array[_head];
            return true;
        }
        item = default;
        return false;
    }

    public bool TryPeekLast([MaybeNullWhen(false)] out T item)
    {
        if (_count > 0) {
            var index = _tail;
            Decrement(ref index);
            item = _array[index];
            return true;
        }
        item = default;
        return false;
    }

    #region Modifiers

    public void Enqueue(T item)
    {
        if (_count == _maxCount) {
            ThrowFullIfBehaviourRequires();
            _array[_tail] = item;
            Increment(ref _head);
            Increment(ref _tail);
        }
        // If current array is full, and capacity hasn't reach _maxCount,
        // extend array length
        else if (_count == _array.Length) {
            var capacity = _array.Length;
            GrowAndCopy(capacity + 1);
            _head = 0;
            _tail = capacity;
            _array[_tail] = item;
            Increment(ref _tail);
            _count++;
        }
        // Normal queue
        else {
            _array[_tail] = item;
            Increment(ref _tail);
            _count++;
        }
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
        Increment(ref _head);
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

        Decrement(ref _tail);
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

    public bool Contains(T item)
    {
        if (_count == 0)
            return false;

        if (_head < _tail)
            return Array.IndexOf(_array, item, _head, _tail - _head) >= 0;
        else
            return Array.IndexOf(_array, item, _head, MaxCount - _head) >= 0
                || Array.IndexOf(_array, item, 0, _head) >= 0;
    }

    public void CopyTo(T[] array, int arrayIndex)
        => AsSpan().CopyTo(array.AsSpan(arrayIndex));

    public Enumerator GetEnumerator() => new(this);

    bool ICollection<T>.IsReadOnly => false;
    void ICollection<T>.Add(T item) => Enqueue(item);
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
    bool ICollection<T>.Remove(T item) => false;
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    [DebuggerStepThrough]
    private void ThrowFullIfBehaviourRequires()
    {
        if (_fullBehaviour is RingQueueFullBehaviour.Throw)
            throw new InvalidOperationException("Ring queue is full");
    }

    private void GrowAndCopy(int expectedCapacity)
    {
        Debug.Assert(expectedCapacity > _array.Length);
        Debug.Assert(expectedCapacity <= _maxCount);
        var span = AsSpan();
        ArrayGrowHelper.GrowNonMove(ref _array, expectedCapacity, _maxCount);
        span.CopyTo(_array.AsSpan(0, span.Length));
    }

    [DebuggerStepThrough]
    private void Increment(ref int index)
    {
        index++;
        if (index >= _array.Length)
            index -= _array.Length;
    }

    [DebuggerStepThrough]
    private void Decrement(ref int index)
    {
        if (index == 0)
            index = _array.Length - 1;
        else
            index--;
    }

    public struct Enumerator : IEnumerator<T>
    {
        private readonly RingQueue<T> _queue;
        private readonly int _version;
        private int _index;
        private T? _current;

        public readonly T Current => _current!;

#nullable disable
        readonly object IEnumerator.Current => Current;
#nullable restore

        internal Enumerator(RingQueue<T> queue)
        {
            _queue = queue;
            _version = _queue._version;
            _index = _queue.Count == 0 ? -1 : _queue._head;
        }

        public bool MoveNext()
        {
            CheckVersion();

            if (_index < 0) {
                _current = default;
                return false;
            }

            // |  ---  |
            if (_queue._head < _queue._tail) {
                _current = _queue._array[_index];
                if (_index + 1 == _queue._tail)
                    _index = -1;
                else
                    _index++;
                return true;
            }
            // |--  --|
            else {
                _current = _queue._array[_index];
                var next = _index;
                _queue.Increment(ref next);
                if (next == _queue._tail)
                    _index = -1;
                else
                    _index = next;
                return true;
            }
        }

        void IEnumerator.Reset()
        {
            CheckVersion();
            _index = _queue.Count == 0 ? -1 : _queue._head;
        }

        private readonly void CheckVersion()
        {
            if (_version != _queue._version)
                TraThrow.CollectionModified();
        }

        public readonly void Dispose() { }
    }
}
