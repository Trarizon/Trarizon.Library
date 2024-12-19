#if !NETSTANDARD2_0

using CommunityToolkit.Diagnostics;
using CommunityToolkit.HighPerformance;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Trarizon.Library.Collections.StackAlloc;
public readonly ref struct ReversedSpan<T>
{
    internal readonly ref T _reference;
    private readonly int _length;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ReversedSpan(ref T reference, int length)
    {
        Debug.Assert(length >= 0);
        _reference = ref reference;
        _length = length;
    }

    public ref T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            Guard.IsLessThan((uint)index, (uint)_length);
            return ref DangerousGetReferenceAt(index);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T DangerousGetReferenceAt(int index)
        => ref Unsafe.Subtract(ref _reference, index);

    public int Length => _length;

    public bool IsEmpty => _length == 0;

    public Span<T> Reverse() => MemoryMarshal.CreateSpan(ref DangerousGetReferenceAt(_length - 1), _length);

    public static implicit operator ReadOnlyReversedSpan<T>(ReversedSpan<T> span)
        => new(in span._reference, span.Length);

    public ReversedSpan<T> Slice(int index, int length)
    {
        Guard.IsLessThanOrEqualTo((ulong)(uint)index + (ulong)(uint)length, (ulong)(uint)_length);
        return new ReversedSpan<T>(ref DangerousGetReferenceAt(index), length);
    }

    public ReversedSpan<T> Slice(int index)
    {
        Guard.IsLessThan((uint)index, (uint)_length);
        return new ReversedSpan<T>(ref DangerousGetReferenceAt(index), _length - index);
    }

    public void CopyTo(Span<T> destination)
    {
        Guard.HasSizeGreaterThanOrEqualTo(destination, Length);

        for (int i = 0; i < Length; i++) {
            destination.DangerousGetReferenceAt(i) = DangerousGetReferenceAt(i);
        }
    }

    public bool TryCopyTo(Span<T> destination)
    {
        if (destination.Length < _length)
            return false;

        for (int i = 0; i < Length; i++) {
            destination.DangerousGetReferenceAt(i) = DangerousGetReferenceAt(i);
        }
        return true;
    }

    public T[] ToArray()
    {
        if (_length == 0)
            return [];

        T[] array = new T[Length];
        CopyTo(array);
        return array;
    }

    public static bool operator ==(ReversedSpan<T> left, ReversedSpan<T> right)
        => left._length == right._length && Unsafe.AreSame(ref left._reference, ref right._reference);

    public static bool operator !=(ReversedSpan<T> left, ReversedSpan<T> right) => !(left == right);

    public Enumerator GetEnumerator() => new(this);

    public ref struct Enumerator
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
            Span<char> buffer = _length > 1024 ? new char[_length] : stackalloc char[_length];
            new ReadOnlyReversedSpan<char>(in Unsafe.As<T, char>(ref _reference), _length).CopyTo(buffer);
            return buffer.ToString();
        }

        return $"ReversedSpan<{typeof(T).Name}>[{_length}]";
    }
}

#endif
