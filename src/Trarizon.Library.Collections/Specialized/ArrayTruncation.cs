using CommunityToolkit.Diagnostics;
using System.Collections;

namespace Trarizon.Library.Collections.Specialized;
public readonly struct ArrayTruncation<T> : IList<T>, IReadOnlyList<T>
{
    private readonly T[] _array;
    private readonly int _length;

    public ArrayTruncation(T[] array, int length)
    {
        Guard.IsLessThanOrEqualTo(length, array.Length);
        _array = array;
        _length = length;
    }

    public ArrayTruncation(T[] array)
    {
        _array = array;
        _length = array.Length;
    }

    public T[] Array => _array;

    public int Length => _length;

    public T this[int index]
    {
        get {
            Guard.IsLessThan((uint)index, (uint)_length);
            return _array[index];
        }
        set {
            Guard.IsLessThan((uint)index, (uint)_length);
            _array[index] = value;
        }
    }

    public ArrayTruncation<T> Slice(int length) => new(_array, length);
    public ArraySegment<T> AsSegment() => new(_array, 0, _length);
    public Memory<T> AsMemory() => new(_array, 0, _length);
    public Span<T> AsSpan() => new(_array, 0, _length);

    public Enumerator GetEnumerator() => new(this);

    public int IndexOf(T item) => System.Array.IndexOf(_array, item, 0, _length);
    public bool Contains(T item) => System.Array.IndexOf(_array, item, 0, _length) >= 0;

    public void CopyTo(T[] array, int arrayIndex) => System.Array.Copy(_array, 0, array, arrayIndex, _length);
    public void CopyTo(ArrayTruncation<T> array)
    {
        if (array._length < _length)
            ThrowHelper.ThrowInvalidOperationException("Destination too short");
        CopyTo(array._array, 0);
    }

    #region Interface

    int ICollection<T>.Count => Length;
    int IReadOnlyCollection<T>.Count => Length;

    bool ICollection<T>.IsReadOnly => false;

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    void IList<T>.Insert(int index, T item) => ThrowHelper.ThrowInvalidOperationException();
    void IList<T>.RemoveAt(int index) => ThrowHelper.ThrowInvalidOperationException();
    void ICollection<T>.Add(T item) => ThrowHelper.ThrowInvalidOperationException();
    void ICollection<T>.Clear() => ThrowHelper.ThrowInvalidOperationException();
    bool ICollection<T>.Remove(T item) => ThrowHelper.ThrowInvalidOperationException<bool>();

    #endregion

    public struct Enumerator : IEnumerator<T>
    {
        private readonly T[] _array;
        private readonly int _length;
        private int _index;
        private T? _current;

        internal Enumerator(ArrayTruncation<T> seg)
        {
            _array = seg._array;
            _length = seg._length;
        }

        readonly public T Current => _current!;

        public bool MoveNext()
        {
            if (_index < 0)
                return false;

            if (_index < _length) {
                _current = _array[_index];
                _index++;
                return true;
            }
            else {
                _index = -1;
                _current = default;
                return false;
            }
        }

        public void Reset() => _index = 0;

#nullable disable
        readonly object IEnumerator.Current => Current;
#nullable restore

        readonly void IDisposable.Dispose() { }
    }
}
