using CommunityToolkit.Diagnostics;
using System.Runtime.CompilerServices;

namespace Trarizon.Library.Collections.StackAlloc;
public readonly ref struct ReadOnlyConcatSpan<T>(ReadOnlySpan<T> first, ReadOnlySpan<T> second)
{
    private readonly ReadOnlySpan<T> _first = first;
    private readonly ReadOnlySpan<T> _second = second;

    public int Length => _first.Length + _second.Length;

    public ref readonly T this[int index]
    {
        get {
            if (index < _first.Length)
                return ref _first[index];
            index -= _first.Length;
            // _second[index] will throw if out of range
            return ref _second[index];
        }
    }

    public ReadOnlyConcatSpan<T> Slice(int startIndex, int length)
    {
        Guard.IsLessThanOrEqualTo(startIndex + length, Length);

        if (length == 0)
            return default;

        if (startIndex < _first.Length) {
            var endIndex = startIndex + length;
            if (endIndex < _first.Length) {
                return new(_first.Slice(startIndex, length), []);
            }
            else {
                endIndex -= _first.Length;
                return new(_first, _second[..endIndex]);
            }
        }
        else {
            startIndex -= _first.Length;
            return new(_second.Slice(startIndex, length), []);
        }
    }

    public ReadOnlyConcatSpan<T> Slice(int startIndex)
    {
        if (startIndex < _first.Length) {
            return new(_first[startIndex..], _second);
        }
        else {
            var start = startIndex - _first.Length;
            return new(_second[start..], []);
        }
    }

    public void CopyTo(Span<T> destination)
    {
        Guard.HasSizeGreaterThanOrEqualTo(destination, Length);

        _first.CopyTo(destination);
        _second.CopyTo(destination[_first.Length..]);
    }

    public T[] ToArray()
    {
        if (Length == 0)
            return [];

        T[] array = new T[Length];
        CopyTo(array);
        return array;
    }

    public Enumerator GetEnumerator() => new(this);

    public ref struct Enumerator
    {
        private readonly ReadOnlyConcatSpan<T> _span;
        private int _index;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Enumerator(ReadOnlyConcatSpan<T> span)
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
