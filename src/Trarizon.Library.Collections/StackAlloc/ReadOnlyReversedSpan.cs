using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Trarizon.Library.Collections.Helpers;

namespace Trarizon.Library.Collections.StackAlloc;
public readonly ref struct ReadOnlyReversedSpan<T>
{
#if NETSTANDARD
    private readonly ReadOnlySpan<T> _span;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ReadOnlyReversedSpan(ReadOnlySpan<T> span) => _span = span;
#else
    private readonly ref readonly T _reference;
    private readonly int _length;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ReadOnlyReversedSpan(in T reference, int length)
    {
        Debug.Assert(length >= 0);
        _reference = ref reference;
        _length = length;
    }
#endif

    public ref readonly T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            Throws.ThrowIfIndexGreaterThanOrEqual(index, Length);
            return ref DangerousGetReferenceAt(index);
        }
    }

#if NETSTANDARD
    public int Length => _span.Length;
    public bool IsEmpty => _span.IsEmpty;
#else
    public int Length => _length;
    public bool IsEmpty => _length == 0;
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref readonly T DangerousGetReferenceAt(int index)
#if NETSTANDARD
        => ref Unsafes.GetReferenceAt(_span, Length - 1 - index);
#else
        => ref Unsafe.Subtract(ref Unsafe.AsRef(in _reference), index);
#endif

    public ReadOnlySpan<T> Reverse()
#if NETSTANDARD
        => _span;
#else
        => MemoryMarshal.CreateReadOnlySpan(in DangerousGetReferenceAt(_length - 1), _length);
#endif


    public ReadOnlyReversedSpan<T> Slice(int index, int length)
    {
#if NETSTANDARD
        var start = Length - 1 - length;
        return new ReadOnlyReversedSpan<T>(_span.Slice(start, length));
#else
        Throws.ThrowIfGreaterThan((uint)index + (uint)length, (uint)Length);
        return new ReadOnlyReversedSpan<T>(in DangerousGetReferenceAt(index), length);
#endif
    }

    public ReadOnlyReversedSpan<T> Slice(int index)
    {
#if NETSTANDARD
        var length = Length - index;
        return new ReadOnlyReversedSpan<T>(_span[..length]);
#else
        Throws.ThrowIfIndexGreaterThanOrEqual(index, _length);
        return new ReadOnlyReversedSpan<T>(in DangerousGetReferenceAt(index), _length - index);
#endif
    }

    public void CopyTo(Span<T> destination)
    {
        Throws.ThrowIfLessThan(destination.Length, Length);

        for (int i = 0; i < Length; i++) {
            Unsafes.GetReferenceAt(destination, i) = DangerousGetReferenceAt(i);
        }
    }


    public bool TryCopyTo(Span<T> destination)
    {
        if (destination.Length < Length)
            return false;
        destination.GetEnumerator();
        for (int i = 0; i < Length; i++) {
            Unsafes.GetReferenceAt(destination, i) = DangerousGetReferenceAt(i);
        }
        return true;
    }

    public T[] ToArray()
    {
        if (IsEmpty)
            return [];

        T[] array = new T[Length];
        CopyTo(array);
        return array;
    }

    public static bool operator ==(ReadOnlyReversedSpan<T> left, ReadOnlyReversedSpan<T> right)
#if NETSTANDARD
        => left._span == right._span;
#else
        => left._length == right._length && Unsafe.AreSame(in left._reference, in right._reference);
#endif

    public static bool operator !=(ReadOnlyReversedSpan<T> left, ReadOnlyReversedSpan<T> right) => !(left == right);

    public Enumerator GetEnumerator() => new(this);

    public ref struct Enumerator
    {
        private readonly ReadOnlyReversedSpan<T> _span;
        private int _index;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Enumerator(ReadOnlyReversedSpan<T> span)
        {
            _span = span;
            _index = -1;
        }

        public readonly ref readonly T Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _index < 0 ? ref Unsafe.NullRef<T>() : ref _span.DangerousGetReferenceAt(_index);
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

    public override string ToString()
    {
        if (typeof(T) == typeof(char)) {
#if NETSTANDARD
            var buffer = new char[Length];
            this.CopyTo(Unsafe.As<T[]>(buffer));
            return buffer.ToString();
#else
            Span<char> buffer = Length > 1024 ? new char[Length] : stackalloc char[Length];
            new ReadOnlyReversedSpan<char>(in Unsafe.As<T, char>(ref Unsafe.AsRef(in _reference)), _length).CopyTo(buffer);
            return buffer.ToString();
#endif
        }

        return $"ReadOnlyReversedSpan<{typeof(T).Name}>[{Length}]";
    }
}
