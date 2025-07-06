using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Trarizon.Library.Collections.Helpers;

namespace Trarizon.Library.Collections.StackAlloc;
public readonly ref partial struct ReversedSpan<T>
{
#if NETSTANDARD
    private readonly Span<T> _span;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ReversedSpan(Span<T> span) => _span = span;
#else
    private readonly ref T _reference;
    private readonly int _length;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ReversedSpan(ref T reference, int length)
    {
        Debug.Assert(length >= 0);
        _reference = ref reference;
        _length = length;
    }
#endif

    public ref T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            Throws.ThrowIfGreaterThanOrEqual(index, Length);
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
    public ref T DangerousGetReferenceAt(int index)
#if NETSTANDARD
        => ref Unsafes.GetReferenceAt(_span, Length - 1 - index);
#else
        => ref Unsafe.Subtract(ref _reference, index);
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T DangerousGetReference()
#if NETSTANDARD
        => ref DangerousGetReferenceAt(0);
#else
        => ref _reference;
#endif

    public Span<T> Reverse()
#if NETSTANDARD
        => _span;
#else
        => MemoryMarshal.CreateSpan(ref DangerousGetReferenceAt(_length - 1), _length);
#endif

    public ReversedSpan<T> Slice(int index, int length)
    {
#if NETSTANDARD
        var start = Length - 1 - length;
        return new ReversedSpan<T>(_span.Slice(start, length));
#else
        Throws.ThrowIfGreaterThan((uint)index + (uint)length, (uint)_length);
        return new ReversedSpan<T>(ref DangerousGetReferenceAt(index), length);
#endif
    }

    public ReversedSpan<T> Slice(int index)
    {
#if NETSTANDARD
        var length = Length - index;
        return new ReversedSpan<T>(_span[..length]);
#else
        Throws.ThrowIfGreaterThanOrEqual(index, _length);
        return new ReversedSpan<T>(ref DangerousGetReferenceAt(index), _length - index);
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

    public static implicit operator ReadOnlyReversedSpan<T>(ReversedSpan<T> span)
#if NETSTANDARD
        => new(span._span);
#else
        => new(in span._reference, span.Length);
#endif

    public static bool operator ==(ReversedSpan<T> left, ReversedSpan<T> right)
#if NETSTANDARD
        => left._span == right._span;
#else
        => left._length == right._length && Unsafe.AreSame(ref left._reference, ref right._reference);
#endif

    public static bool operator !=(ReversedSpan<T> left, ReversedSpan<T> right) => !(left == right);

    public Enumerator GetEnumerator() => new(this);

    public ref partial struct Enumerator
    {
        private readonly ReversedSpan<T> _span;
        private int _index;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Enumerator(ReversedSpan<T> span)
        {
            _span = span;
            _index = -1;
        }

        public readonly ref T Current
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
            return ((ReadOnlyReversedSpan<T>)this).ToString();
        }

        return $"ReversedSpan<{typeof(T).Name}>[{Length}]";
    }
}

