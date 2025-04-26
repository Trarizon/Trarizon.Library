using CommunityToolkit.Diagnostics;
using CommunityToolkit.HighPerformance;
using System.Collections;
using Trarizon.Library.Buffers;
using Trarizon.Library.Common;
using Trarizon.Library.Mathematics;

namespace Trarizon.Library.Collections.Specialized;
public class AutoAllocList<T>(IObjectAllocator<T> allocator)
    : AutoAllocList<T, IObjectAllocator<T>>(allocator)
    where T : class
{ }

public class AutoAllocList<T, TAllocator> : IList<T>, IReadOnlyList<T>
    where T : class
    where TAllocator : IObjectAllocator<T>
{
    private readonly List<T> _items;
    private readonly TAllocator _allocator;

    public T this[int index] => _items[index];

    public int Count => _items.Count;

    protected AutoAllocList(TAllocator allocator)
    {
        _items = new();
        _allocator = allocator;
    }

    public int IndexOf(T item) => _items.IndexOf(item);

    public void Add(out T item)
    {
        item = Allocate();
        _items.Add(item);
    }

    public void Insert(int index, out T item)
    {
        item = Allocate();
        _items.Insert(index, item);
    }

    public bool Remove(T item)
    {
        if (_items.Remove(item)) {
            Release(item);
            return true;
        }
        return false;
    }

    public void RemoveAt(int index)
    {
        var item = _items[index];
        _items.RemoveAt(index);
        Release(item);
    }

    public void RemoveAt(Index index) => RemoveAt(index.GetOffset(Count));

    public void RemoveRange(int index, int length)
    {
        TraIndex.ValidateSliceArgs(index, length, _items.Count);
        var range = _items.AsSpan().Slice(index, length);
        Release(range);
        RemoveRange(index, length);
    }

    public void RemoveRange(Range range)
    {
        var (index, length) = range.GetOffsetAndLength(_items.Count);
        RemoveRange(index, length);
    }

    public void Clear()
    {
        Release(_items.AsSpan());
        _items.Clear();
    }

    public void MoveTo(Index from, Index to)
    {
        _items.AsSpan().MoveTo(from, to);
    }

    public void SetCount(int count)
    {
        if (count <= 0) {
            Clear();
        }
        else if (count < _items.Count) {
            RemoveRange(count..);
        }
        else if (count > _items.Count) {
            var addCount = count - _items.Count;
            for (int i = 0; i < addCount; i++) {
                Add(out _);
            }
        }
    }

    public bool Contains(T item) => _items.Contains(item);

    public ResettingScope EnterResetting() => new(this);

    public Enumerator GetEnumerator() => new(this);

    private T Allocate() => _allocator.Allocate();
    private void Release(T item) => _allocator.Release(item);

    protected virtual void Release(ReadOnlySpan<T> items)
    {
        foreach (var item in items) {
            Release(item);
        }
    }

    #region Interface

    bool ICollection<T>.IsReadOnly => ((ICollection<T>)_items).IsReadOnly;

    T IList<T>.this[int index]
    {
        get => this[index];
        set => ThrowHelper.ThrowInvalidOperationException("Cannot set item of auto-alloc list");
    }

    void ICollection<T>.Add(T item) => ThrowHelper.ThrowInvalidOperationException("Cannot set item of auto-alloc list");
    void ICollection<T>.CopyTo(T[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);
    void IList<T>.Insert(int index, T item) => ThrowHelper.ThrowInvalidOperationException("Cannot set item of auto-alloc list");
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion

    public struct ResettingScope : IDisposable
    {
        private readonly AutoAllocList<T, TAllocator> _list;
        private int _count;

        internal ResettingScope(AutoAllocList<T, TAllocator> list)
        {
            _list = list;
            _count = 0;
        }

        public void Add(out T item)
        {
            if (_count < _list.Count) {
                item = _list[_count];
                _count++;
            }
            else {
                _list.Add(out item);
                _count++;
            }
        }

        public readonly void Dispose()
        {
            if (_count < _list.Count) {
                _list.SetCount(_count);
            }
        }
    }

    public struct Enumerator : IEnumerator<T>
    {
        private List<T>.Enumerator _enumerator;

        internal Enumerator(AutoAllocList<T, TAllocator> list)
        {
            _enumerator = list._items.GetEnumerator();
        }

        public T Current => _enumerator.Current;

        object IEnumerator.Current => Current;

        public bool MoveNext() => _enumerator.MoveNext();

        void IEnumerator.Reset() => ((IEnumerator)_enumerator).Reset();
        readonly void IDisposable.Dispose() => ((IDisposable)_enumerator).Dispose();
    }
}
