using System.Collections;
using System.Runtime.CompilerServices;
using Trarizon.Library.Collections.AllocOpt.Providers;
using Trarizon.Library.Collections.StackAlloc;

namespace Trarizon.Library.Collections.Generic;
public class RingQueue<T>(int maxCount, RingQueue<T>.CollectionFullBehaviour fullBehaviour = RingQueue<T>.CollectionFullBehaviour.Overwrite)
    : ICollection<T>, IReadOnlyCollection<T>
{
    private readonly AllocOptGrowableArrayProvider<T> _array = new();
    private int _head = 0;
    private int _tail = 0;
    private readonly CollectionFullBehaviour _fullBehaviour = fullBehaviour;
    private readonly int _maxCount = maxCount;
    private int _version = 0;

    public int Count
    {
        get {
            if (_head < _tail) {
                return _tail - _head;
            }
            if (_tail == _head) {
                // If empty, items of _array will be cleared, so returns 0,
                // otherwise, returns MaxCount
                return _array.Count;
            }

            return MaxCount - _head + _tail;
        }
    }

    public int Capacity => _array.Array.Length;

    public int MaxCount => _maxCount;

    public bool IsFull => _head == _tail && _array.Count > 0;

    public bool IsEmpty => _head == _tail && _array.Count == 0;

    public ReadOnlyConcatSpan<T> AsSpan()
    {
        var array = _array.Array;
        // |  ---  |
        if (_head < _tail)
            return new(array.AsSpan(_head.._tail), default);
        // Empty
        else if (_head == _tail && _array.Count == 0)
            return default;
        // |--  --|
        else
            return new(array.AsSpan(_head), array.AsSpan(0, _tail));
    }

    public void Enqueue(T item)
    {
        if (IsFull) {
            ThrowFullIfBehaviourRequires();
            _array.Array[_tail] = item;
            Increment(ref _head);
            Increment(ref _tail);
        }
        else if (_array.Count == MaxCount) {
            _array.Array[_tail] = item;
            Increment(ref _tail);
        }
        else {
            _array.Add(item);
            Increment(ref _tail);
        }
        _version++;
    }

    public void DequeueFirst()
    {
        if (IsEmpty)
            TraThrow.NoElement();

        _array.Array[_head] = default!;
        Increment(ref _head);
        _version++;
    }

    public void DequeueLast()
    {
        if (IsEmpty)
            TraThrow.NoElement();

        _array.Array[_tail] = default!;
        Decrement(ref _tail);
        _version++;
    }

    public void Clear()
    {
        _array.Clear();
        _head = 0;
        _tail = 0;
        _version++;

        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>()) {
            Array.Clear(_array.Array);
        }
    }

    public bool Contains(T item)
    {
        var array = _array.Array;
        if (_head < _tail)
            return Array.IndexOf(array, item, _head, _tail - _head) >= 0;
        else if (_head == _tail && _array.Count == 0)
            return false;
        else
            return Array.IndexOf(array, item, _head, MaxCount - _head) >= 0
                || Array.IndexOf(array, item, 0, _head) >= 0;
    }

    public void CopyTo(T[] array, int arrayIndex)
        => AsSpan().CopyTo(array.AsSpan(arrayIndex));

    public Enumerator GetEnumerator() => new(this);

    bool ICollection<T>.IsReadOnly => false;
    void ICollection<T>.Add(T item) => Enqueue(item);
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
    bool ICollection<T>.Remove(T item) => false;
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private void ThrowFullIfBehaviourRequires()
    {
        if (_fullBehaviour is CollectionFullBehaviour.Throw)
            throw new InvalidOperationException("Ring queue is full");
    }

    private void Increment(ref int index)
    {
        index++;
        if (index >= MaxCount)
            index -= MaxCount;
    }

    private void Decrement(ref int index)
    {
        if (index == 0)
            index = MaxCount;
        else
            index--;
    }

    public struct Enumerator : IEnumerator<T>
    {
        private readonly RingQueue<T> _queue;
        private readonly int _version;
        private int _index = -1;
        private T? _current;

        public readonly T Current => _current!;

        readonly object? IEnumerator.Current => Current;

        internal Enumerator(RingQueue<T> queue)
        {
            _queue = queue;
            _version = _queue._version;

            if (_queue.IsEmpty)
                _index = -1;
            else
                _index = _queue._head;
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
                _current = _queue._array.Array[_index];
                if (_index + 1 == _queue._tail)
                    _index = -1;
                else
                    _index++;
                return true;
            }
            // |--  --|
            else {
                if (_index + 1 == _queue._tail)
                    _index = -1;
                else
                    _index++;
                return true;
            }
        }

        void IEnumerator.Reset()
        {
            CheckVersion();
            if (_queue.IsEmpty)
                _index = -1;
            else
                _index = _queue._head;
        }

        private readonly void CheckVersion()
        {
            if (_version != _queue._version)
                TraThrow.CollectionModified();
            return;
        }

        public readonly void Dispose() { }
    }

    public enum CollectionFullBehaviour
    {
        Overwrite = 0,
        Throw,
    }
}
