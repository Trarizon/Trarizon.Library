using System.Runtime.CompilerServices;

namespace Trarizon.Library.Collections.StackAlloc;
public readonly ref struct ReversedSpan<T>
{
    private readonly Span<T> _span;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ReversedSpan(Span<T> span)
        => _span = span;

    public static implicit operator ReversedReadOnlySpan<T>(ReversedSpan<T> span)
        => new(span._span);

    public int Length => _span.Length;

    public ref T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref _span[_span.Length - 1 - index];
    }

    public Span<T> Reverse() => _span;

    public ReversedSpan<T> Slice(int index, int length)
        => new(_span.Slice(Length - index - length, length));

    public ReversedSpan<T> Slice(int index)
        => new(_span[..(Length - index)]);

    public void CopyTo(Span<T> span)
    {
        if (span.Length < Length)
            ThrowHelper.ThrowArgument("Size of destination span should not small than source", nameof(span));

        for (int i = 0; i < Length; i++) {
            span[i] = this[i];
        }
    }

    public T[] ToArray()
    {
        if (_span.Length == 0)
            return [];

        T[] array = new T[Length];
        for (int i = 0; i < Length; i++) {
            array[i] = this[i];
        }
        return array;
    }

    public Enumerator GetEnumerator() => new(_span);

    public ref struct Enumerator
    {
        private readonly Span<T> _span;
        private int _index;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Enumerator(Span<T> span)
        {
            _span = span;
            _index = span.Length;
        }

        public readonly ref T Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _span[_index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            int index = _index - 1;
            if (index >= 0) {
                _index = index;
                return true;
            }
            return false;
        }
    }
}
