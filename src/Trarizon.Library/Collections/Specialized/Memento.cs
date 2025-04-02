using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Trarizon.Library.Collections.StackAlloc;

namespace Trarizon.Library.Collections.Specialized;
public class Memento<T>
{
    private T[] _array;
    private int _activeCount;
    private int _count;
    private readonly int _maxCount;
    private int _head;
    private int _tail;
    private int _index; // tail of active

    private int _version;

    public Memento(int maxCapacity)
    {
        _array = [];
        _maxCount = maxCapacity;
        _activeCount = _count = 0;
        _head = _tail = _index = 0;
    }

    public int ActiveCount => _activeCount;

    public int Count => _count;

    public int MaxCount => _maxCount;

    public ReadOnlyConcatSpan<T> AsSpan()
    {
        if (_count == 0)
            return default;

        if (_head < _tail)
            return new(_array.AsSpan(_head.._tail), default);
        else
            return new(_array.AsSpan(_head), _array.AsSpan(0, _tail));
    }

    public T Peek()
    {
        if (!TryPeek(out var item))
            TraThrow.NoElement();

        return item;
    }

    public bool TryPeek([MaybeNullWhen(false)] out T activeItem)
    {
        if (ActiveCount == 0) {
            activeItem = default;
            return false;
        }

        var index = _index;
        Decrement(ref index);
        activeItem = _array[index];
        return true;
    }

    #region Modifiers

    public void Add(T item)
    {
        if (_activeCount != _count) {
            Debug.Assert(_activeCount < _count);
            Debug.Assert(_activeCount < _maxCount);
            _array[_index] = item;
            Increment(ref _index);
            _tail = _index;
            _activeCount++;
            _count = _activeCount;
            _version++;
            return;
        }

        Debug.Assert(_index == _tail);
        if (_activeCount == MaxCount) {
            _array[_index] = item;
            Increment(ref _index);
            _head = _tail = _index;
        }
        // If current array is full, and capacity hasnt reach _maxCount
        // extend array length
        else if (_activeCount == _array.Length) {
            var capacity = _array.Length;
            GrowAndCopy(capacity + 1);
            _head = 0;
            _index = capacity;
            _array[_index] = item;
            Increment(ref _index);
            _tail = _index;
            _count++;
            _activeCount++;
        }
        // Normal queue
        else {
            _array[_index] = item;
            Increment(ref _index);
            _tail = _index;
            _count++;
            _activeCount++;
        }

        _version++;
    }

    public void Rollback(out T item)
    {
        if (!TryRollback(out item!))
            TraThrow.NoElement();
    }

    public bool TryRollback([MaybeNullWhen(false)] out T item)
    {
        if (_activeCount == 0) {
            item = default;
            return false;
        }
        _activeCount--;
        Decrement(ref _index);
        item = _array[_index];

        _version++;
        return true;
    }

    public void Reapply(out T item)
    {
        if (!TryReapply(out item!))
            TraThrow.NoElement();
    }

    public bool TryReapply([MaybeNullWhen(false)] out T item)
    {
        if (_activeCount == _count) {
            item = default;
            return false;
        }

        _activeCount++;
        item = _array[_index];
        Increment(ref _index);

        _version++;
        return true;
    }

    public void Clear()
    {
        if (_count == 0)
            return;
        ArrayGrowHelper.FreeManaged(_array);
        _head = _index = _tail = 0;
        _count = _activeCount = 0;
        _version++;
    }

    #endregion

    #region Enumeration

    public Enumerator GetEnumerator() => new(this);

    public TaggedEnumerable EnumerateTagged() => new(this);

    #endregion

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

    public struct Enumerator
    {
        private readonly Memento<T> _memento;
        private readonly int _version;
        private int _index;
        private T? _current;
        private bool _isActive;

        internal Enumerator(Memento<T> memento)
        {
            _memento = memento;
            _index = _memento.Count == 0 ? -1 : _memento._head;
            _isActive = true;
            _version = _memento._version;
        }

        public readonly T Current => _current!;
        public readonly bool IsActive => _isActive;

        public bool MoveNext()
        {
            CheckVersion();

            if (_index < 0) {
                _current = default;
                return false;
            }

            // | --- |
            if (_memento._head < _memento._tail) {
                _current = _memento._array[_index];
                if (_isActive && _index == _memento._index) {
                    _isActive = false;
                }

                var index = _index + 1;
                if (index == _memento._tail)
                    _index = -1;
                else
                    _index = index;
                return true;
            }
            // |--  --|
            else {
                _current = _memento._array[_index];
                // If _index == _memento._tail, the array is full, and all items are active
                if (_isActive && _index == _memento._index && _index != _memento._tail)
                    _isActive = false;

                var index = _index;
                _memento.Increment(ref index);
                if (index == _memento._tail) {
                    _index = -1;

                }
                else
                    _index = index;
                return true;
            }
        }

        private readonly void CheckVersion()
        {
            if (_version != _memento._version)
                TraThrow.CollectionModified();
        }

        internal void SetEnd() => _index = -1;
    }

    public readonly struct TaggedEnumerable
    {
        private readonly Memento<T> _memento;

        internal TaggedEnumerable(Memento<T> memento)
        {
            _memento = memento;
        }

        public TaggedEnumerator GetEnumerator() => new(_memento);

        public struct TaggedEnumerator
        {
            private Enumerator _enumerator;

            internal TaggedEnumerator(Memento<T> memento)
            {
                _enumerator = memento.GetEnumerator();
            }

            public readonly (bool IsActive, T Item) Current => (_enumerator.IsActive, _enumerator.Current);

            public bool MoveNext() => _enumerator.MoveNext();
        }
    }

    public readonly struct ActiveEnumerable
    {
        private readonly Memento<T> _memento;
        internal ActiveEnumerable(Memento<T> memento)
        {
            _memento = memento;
        }
        public ActiveEnumerator GetEnumerator() => new(_memento);
        public struct ActiveEnumerator
        {
            private Enumerator _enumerator;
            internal ActiveEnumerator(Memento<T> memento)
            {
                _enumerator = memento.GetEnumerator();
            }
            public readonly T Current => _enumerator.Current;
            public bool MoveNext()
            {
                if (_enumerator.MoveNext()) {
                    if (_enumerator.IsActive)
                        return true;
                    else {
                        _enumerator.SetEnd();
                        return false;
                    }
                }
                return false;
            }
        }
    }
}
