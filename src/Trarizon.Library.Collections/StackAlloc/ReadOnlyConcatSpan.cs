using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Trarizon.Library.Collections.Helpers;

namespace Trarizon.Library.Collections.StackAlloc;
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly ref struct ReadOnlyConcatSpan<T>(ReadOnlySpan<T> first, ReadOnlySpan<T> second)
{
    private readonly ReadOnlySpan<T> _first = first;
    private readonly ReadOnlySpan<T> _second = second;

    public ReadOnlySpan<T> First => _first;

    public ReadOnlySpan<T> Second => _second;

    public int Length => _first.Length + _second.Length;

    public ref readonly T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            if (index < _first.Length)
                return ref Unsafes.GetReferenceAt(_first, index);
            index -= _first.Length;
            // _second[index] will throw if out of range
            return ref _second[index];
        }
    }

    public bool IsEmpty => _first.IsEmpty && _second.IsEmpty;

    public static bool operator ==(ReadOnlyConcatSpan<T> left, ReadOnlyConcatSpan<T> right)
        => left._first == right._second && left._second == right._second;

    public static bool operator !=(ReadOnlyConcatSpan<T> left, ReadOnlyConcatSpan<T> right) => !(left == right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlyConcatSpan<T> Slice(int startIndex, int length)
    {
        Throws.ThrowIfGreaterThan(startIndex + length, Length);

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyTo(Span<T> destination)
    {
        Throws.ThrowIfLessThan(destination.Length, Length);

        _first.CopyTo(destination);
        _second.CopyTo(destination[_first.Length..]);
    }

    public bool TryCopyTo(Span<T> destination)
    {
        if (destination.Length >= Length) {
            _first.CopyTo(destination);
            _second.CopyTo(destination[_first.Length..]);
            return true;
        }
        return false;
    }

    public T[] ToArray()
    {
        if (Length == 0)
            return [];

        T[] array = new T[Length];
        CopyTo(array);
        return array;
    }

    public override string ToString()
    {
        if (typeof(T) == typeof(char)) {
#if NETSTANDARD2_0
            return $"{_first.ToString()}{_second.ToString()}";
#else
            var buffer = (stackalloc char[Length]);

            var first = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<T, char>(ref MemoryMarshal.GetReference(_first)), _first.Length);
            var second = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<T, char>(ref MemoryMarshal.GetReference(_second)), _second.Length);
            first.CopyTo(buffer);
            second.CopyTo(buffer[first.Length..]);
            return new string(buffer);
#endif
        }
        return $"ReadOnlyConcatSpan<{typeof(T).Name}>[{Length}]";
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

#pragma warning disable CS0809

    [Obsolete("Not support", true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override bool Equals(object? obj) => throw new NotImplementedException();

    [Obsolete("Not support", true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override int GetHashCode() => throw new NotImplementedException();

#pragma warning restore CS0809
}
