using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Trarizon.Library.CodeAnalysis.MemberAccess;
using Trarizon.Library.Collections.StackAlloc;

namespace Trarizon.Library.Collections.AllocOpt;
[CollectionBuilder(typeof(AllocOptCollectionBuilder), nameof(AllocOptCollectionBuilder.CreateQueue))]
public struct AllocOptQueue<T> : ICollection<T>, IReadOnlyCollection<T>
{
    private AllocOptDeque<T> _deque;

    public AllocOptQueue()
    {
        _deque = [];
    }

    public AllocOptQueue(int capacity)
    {
        _deque = new(capacity);
    }

    [FriendAccess(typeof(AllocOptCollectionBuilder))]
    internal AllocOptQueue(T[] items, int head, int tail)
        => _deque = AllocOptCollectionBuilder.AsDeque(items, head, tail);

    #region Accessors

    public readonly bool IsEmpty => _deque.IsEmpty;

    public readonly int Count => _deque.Count;

    public readonly int Capacity => _deque.Capacity;

    public readonly T Peek() => _deque.PeekFirst();

    public readonly ReadOnlyRingSpan<T> Peek(int count) => _deque.PeekFirst(count);

    public readonly bool TryPeek([MaybeNullWhen(false)] out T item) => _deque.TryPeekFirst(out item);

    public readonly bool TryPeek(int count, out ReadOnlyRingSpan<T> items) => _deque.TryPeekFirst(count, out items);

    [SuppressMessage("Style", "IDE0305", Justification = "<挂起>")]
    public readonly T[] ToArray() => _deque.ToArray();

    public readonly T[] GetUnderlyingArray() => _deque.GetUnderlyingArray();

    public readonly AllocOptDeque<T>.Enumerator GetEnumerator() => _deque.GetEnumerator();

    #endregion

    #region Builders

    public void Enqueue(T item) => _deque.EnqueueLast(item);

    public void EnqueueRange<TEnumerable>(TEnumerable collection) where TEnumerable : IEnumerable<T> => _deque.EnqueueRangeLast(collection);

    public void EnqueueCollection<TCollection>(TCollection collection) where TCollection : ICollection<T> => _deque.EnqueueCollectionLast(collection);

    public void EnqueueRange(ReadOnlySpan<T> items) => _deque.EnqueueRangeLast(items);

    public T Dequeue() => _deque.DequeueFirst();

    public void Dequeue(int count) => _deque.DequeueFirst(count);

    public bool TryDequeue([MaybeNullWhen(false)] out T item) => _deque.TryDequeueFirst(out item);

    void ICollection<T>.Clear() => _deque.Clear();

    public readonly void FreeUnreferenced() => _deque.FreeUnreferenced();

    public void EnsureCapacity(int capacity) => _deque.EnsureCapacity(capacity);

    #endregion

    #region Interface methods

    readonly bool ICollection<T>.IsReadOnly => false;

    public readonly bool Contains(T item) => _deque.Contains(item);
    public readonly void CopyTo(T[] array, int arrayIndex) => _deque.CopyTo(array, arrayIndex);

    // Mirror from AODeque
    void ICollection<T>.Add(T item) => Enqueue(item);
    bool ICollection<T>.Remove(T item)
    {
        if (TryPeek(out var val) && EqualityComparer<T>.Default.Equals(val, item)) {
            Dequeue(1);
            return true;
        }
        return false;

    }

    readonly IEnumerator<T> IEnumerable<T>.GetEnumerator() => new AllocOptDeque<T>.Enumerator.Wrapper(_deque);
    readonly IEnumerator IEnumerable.GetEnumerator() => new AllocOptDeque<T>.Enumerator.Wrapper(_deque);

    #endregion
}
