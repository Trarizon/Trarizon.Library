using System.Runtime.CompilerServices;
using Trarizon.Library.CodeAnalysis.MemberAccess;
using Trarizon.Library.Collections.AllocOpt;

namespace Trarizon.Library.Collections.StackAlloc;
public readonly ref struct ReadOnlyQueueSpan<T>
{
    private readonly Span<T> _firstPart;
    private readonly Span<T> _secondPart;

    [FriendAccess(typeof(AllocOptDeque<>))]
    internal ReadOnlyQueueSpan(T[] underlyingArray, int head, int tail)
    {
        if (tail > head) {
            _firstPart = underlyingArray.AsSpan(head..tail);
            _secondPart = [];
        }
        else {
            _firstPart = underlyingArray.AsSpan(head);
            _secondPart = underlyingArray.AsSpan(0, tail);
        }
    }

    private ReadOnlyQueueSpan(Span<T> firstPart, Span<T> secondPart)
    {
        _firstPart = firstPart;
        _secondPart = secondPart;
    }

    public int Length => _firstPart.Length + _secondPart.Length;

    public ref readonly T this[int index]
    {
        get {
            if (index < _firstPart.Length)
                return ref _firstPart[index];
            index -= _firstPart.Length;
            // _secondPart.Slice() will throw if out of range
            return ref _secondPart[index];
        }
    }

    public ReversedReadOnlyQueueSpan<T> Reverse() => new(this);

    public ReadOnlyQueueSpan<T> Slice(int startIndex, int length)
    {
        if (startIndex < _firstPart.Length) {
            var endIndex = startIndex + length;
            if (endIndex < _firstPart.Length)
                return new(_firstPart.Slice(startIndex, length), []);
            endIndex -= _firstPart.Length;
            // _secondPart.Slice() will throw if out of range
            return new(_firstPart, _secondPart[..endIndex]);
        }
        startIndex -= _firstPart.Length;
        // _secondPart.Slice() will throw if out of range
        return new(_secondPart.Slice(startIndex, length), []);
    }

    public ReadOnlyQueueSpan<T> Slice(int startIndex)
    {
        if (startIndex < _firstPart.Length) {
            return new(_firstPart[startIndex..], _secondPart);
        }
        startIndex -= _firstPart.Length;
        return new(_secondPart[startIndex..], []);
    }

    public void CopyTo(Span<T> span)
    {
        if (span.Length < Length)
            ThrowHelper.ThrowArgument("Size of destination span should not small than source", nameof(span));

        _firstPart.CopyTo(span);
        _secondPart.CopyTo(span[_firstPart.Length..]);
    }

    public T[] ToArray()
    {
        if (Length == 0)
            return [];

        T[] array = new T[Length];
        _firstPart.CopyTo(array);
        _secondPart.CopyTo(array.AsSpan(_firstPart.Length));
        return array;
    }

    public Enumerator GetEnumerator() => new(this);

    public ref struct Enumerator
    {
        private readonly ReadOnlyQueueSpan<T> _span;
        private int _index;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Enumerator(ReadOnlyQueueSpan<T> span)
        {
            _span = span;
            _index = -1;
        }

        public readonly ref readonly T Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _span[_index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            int index = _index + 1;
            if (index < _span.Length) {
                _index = index;
                return true;
            }
            return false;
        }
    }
}
