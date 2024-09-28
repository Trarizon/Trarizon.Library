using CommunityToolkit.Diagnostics;
using System.Runtime.CompilerServices;

namespace Trarizon.Library.Collections.StackAlloc;
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly ref struct ReversedSpan<T>(Span<T> span)
{
    internal readonly Span<T> _span = span;

    public int Length => _span.Length;

    public ref T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref _span[^(1 + index)];
    }

    public Span<T> Reverse() => _span;

    public ReversedSpan<T> Slice(int index, int length)
        => new(_span.Slice(Length - index - length, length));

    public ReversedSpan<T> Slice(int index)
        => new(_span[..(Length - index)]);

    public void CopyTo(Span<T> destination)
    {
        Guard.HasSizeGreaterThanOrEqualTo(destination, Length);

        for (int i = 0; i < Length; i++) {
            destination[i] = this[i];
        }
    }

    public T[] ToArray()
    {
        if (_span.Length == 0)
            return [];

        T[] array = new T[Length];
        CopyTo(array);
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

