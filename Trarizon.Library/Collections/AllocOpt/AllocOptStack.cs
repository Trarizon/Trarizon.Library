using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Trarizon.Library.Collections.Extensions;

namespace Trarizon.Library.Collections.AllocOpt;
[CollectionBuilder(typeof(AllocOptCollectionBuilder), nameof(AllocOptCollectionBuilder.CreateStack))]
public struct AllocOptStack<T>(int capacity) : ICollection<T>, IReadOnlyCollection<T>
{
    private AllocOptList<T> _list = new(capacity);

    public readonly int Count => _list.Count;

    public int Capacity
    {
        readonly get => _list.Capacity;
        set => _list.Capacity = value;
    }

    readonly bool ICollection<T>.IsReadOnly => true;

    #region Accessors

    public readonly T Peek() => _list[^1];

    public readonly bool TryPeek([MaybeNullWhen(false)] out T value)
    {
        if (Count == 0) {
            value = default;
            return false;
        }

        value = _list[^1];
        return true;
    }

    public readonly SpanQuery.ReversedReadOnlySpanQuerier<T> AsSpan() => _list.AsSpan().ReverseSpan();

    public readonly T[] ToArray() => AsSpan().ToArray();

    public readonly T[] GetUnderlyingArray() => _list.GetUnderlyingArray();

    #endregion

    #region Builders

    public void Push(T item) => _list.Add(item);

    internal void PushRange(ReadOnlySpan<T> values) => _list.AddRange(values);

    /// <summary>
    /// Unlike <see cref="Stack{T}"/>, this wont throw exception if stack is empty
    /// </summary>
    public void Pop()
        => _list.RemoveAt(^1);

    public bool TryPop([MaybeNullWhen(false)] out T value)
    {
        if (TryPeek(out value)) {
            Pop();
            return true;
        }
        else {
            return false;
        }
    }

    /// <summary>
    /// This method won't clear elements in underlying array.
    /// Use <see cref="ClearTrailingReferences"/> if you need it.
    /// </summary>
    public void Clear() => _list.Clear();

    public void ClearTrailingReferences() => _list.ClearTrailingReferences();

    #endregion

    public readonly Enumerator GetEnumerator() => new(this);

    void ICollection<T>.Add(T item) => Push(item);
    public readonly bool Contains(T item) => _list.Contains(item);
    public readonly void CopyTo(T[] array, int arrayIndex)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(arrayIndex, array.Length - Count);

        foreach (var item in this) {
            array[arrayIndex++] = item;
        }
    }
    readonly bool ICollection<T>.Remove(T item)
    {
        ThrowHelper.ThrowNotSupport("Cannot remove specific item from stack");
        return default;
    }

    readonly IEnumerator<T> IEnumerable<T>.GetEnumerator() => new Enumerator.Wrapper(this);
    readonly IEnumerator IEnumerable.GetEnumerator() => new Enumerator.Wrapper(this);

    public struct Enumerator
    {
        private readonly AllocOptStack<T> _stack;
        private int _index;

        internal Enumerator(AllocOptStack<T> stack)
        {
            _stack = stack;
            _index = _stack.Count;
        }

        public readonly T Current => _stack._list[_index];

        public bool MoveNext()
        {
            var index = _index - 1;
            if (index >= 0) {
                _index = index;
                return true;
            }
            else {
                return false;
            }
        }

        internal sealed class Wrapper(AllocOptStack<T> stack) : IEnumerator<T>
        {
            private Enumerator _enumerator = stack.GetEnumerator();

            public T Current => _enumerator.Current;

            object? IEnumerator.Current => Current;

            public void Dispose() { }
            public bool MoveNext() => _enumerator.MoveNext();
            public void Reset() => _enumerator._index = _enumerator._stack.Count;
        }
    }
}
