using System.Runtime.CompilerServices;

namespace Trarizon.Library.Collections.Extensions.Helpers;
public readonly ref struct ReversedSpan<T>
{
    private readonly Span<T> _span;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ReversedSpan(Span<T> span)
        => _span = span;

    public readonly ref readonly T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref _span[_span.Length - 1 - index];
    }

    public Span<T> OriginalSpan => _span;

    public int Length => _span.Length;

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

        public readonly ref readonly T Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _span[_index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            int index = _index - 1;
            if (index >= 0)
            {
                _index = index;
                return true;
            }
            return false;
        }
    }
}
