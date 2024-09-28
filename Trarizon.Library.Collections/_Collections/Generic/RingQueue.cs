using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Trarizon.Library.Collections.AllocOpt;

namespace Trarizon.Library.Collections.Generic;
public class RingQueue<T> : ICollection<T>, IReadOnlyCollection<T>
{
    private readonly AllocOptDeque<T> _deque;
    private readonly bool _throwOnFull;
    private int _version;

    public RingQueue(int maxCount, bool throwOnFull = false)
    {
        _deque = new(maxCount);
        _throwOnFull = throwOnFull;
    }

    #region Accessors

    public int Count => _deque.Count;

    public int Capacity => _deque.Capacity;

    public T Peek() => _deque.PeekFirst();

    public bool TryPeek([MaybeNullWhen(false)] out T item) => _deque.TryPeekFirst(out item);

    public T[] ToArray() => _deque.ToArray();

    public Enumerator GetEnumerator() => new(this);

    #endregion

    #region Modifiers

    public void Enqueue(T item)
    {
        if (Count == Capacity) {
            if (_throwOnFull)
                ThrowHelper.ThrowInvalidOperation("Ring queue full");
            _deque.DequeueFirst();
        }
        _deque.EnqueueLast(item);
        _version++;
    }

    public void Dequeue()
    {
        _deque.DequeueFirst();
        _version++;
    }

    public void Clear()
    {
        _deque.Clear();
        _deque.FreeUnreferenced();
        _version++;
    }

    #endregion

    bool ICollection<T>.IsReadOnly => false;

    public bool Contains(T item) => _deque.Contains(item);
    public void CopyTo(T[] array, int arrayIndex) => _deque.CopyTo(array, arrayIndex);

    void ICollection<T>.Add(T item) => Enqueue(item);
    bool ICollection<T>.Remove(T item)
    {
        if (TryPeek(out var val) && EqualityComparer<T>.Default.Equals(val, item)) {
            Dequeue();
            return true;
        }
        return false;
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


    public struct Enumerator : IEnumerator<T>
    {
        private readonly RingQueue<T> _queue;
        private readonly int _version;
        private int _index;
        private T _current;

        internal Enumerator(RingQueue<T> queue)
        {
            _queue = queue;
            _version = _queue._version;
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
            if (_version != _queue._version)
                ThrowHelper.ThrowInvalidOperation("Collection has been changed after enumerator created.");

            if (_index == -2)
                return false;

            var index = _index + 1;

            if (!_queue._deque.TryGetByIndex(index, out var val)) {
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
            if (_version != _queue._version)
                ThrowHelper.ThrowInvalidOperation("Collection has been changed after enumerator created.");
            _index = -1;
        }
    }
}
