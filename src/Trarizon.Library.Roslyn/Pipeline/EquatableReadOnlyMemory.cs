using System.Collections.Immutable;

namespace Trarizon.Library.Roslyn.Pipeline;

public readonly struct EquatableReadOnlyMemory<T>(ReadOnlyMemory<T> memory) : IEquatable<EquatableReadOnlyMemory<T>>
{
    private readonly ReadOnlyMemory<T> _memory = memory;

    public ReadOnlyMemory<T> Memory => _memory;

    public int Length => _memory.Length;

    public bool IsEmpty => _memory.IsEmpty;

    public ReadOnlySpan<T> Span => _memory.Span;

    public bool Equals(EquatableReadOnlyMemory<T> other)
    {
        if (_memory.Equals(other._memory))
            return true;

        if (_memory.Length != other._memory.Length)
            return false;

        var span = _memory.Span;
        var otherSpan = other._memory.Span;
        for (var i = 0; i < span.Length; i++)
        {
            if (!EqualityComparer<T>.Default.Equals(span[i], otherSpan[i]))
                return false;
        }
        return true;
    }

    public override bool Equals(object obj) => obj is EquatableReadOnlyMemory<T> other && Equals(other);

    public override int GetHashCode()
    {
        HashCode hc = new();
        foreach (var it in Span)
            hc.Add(it);
        return hc.ToHashCode();
    }

    public static bool operator ==(EquatableReadOnlyMemory<T> a, EquatableReadOnlyMemory<T> b) => a.Equals(b);
    public static bool operator !=(EquatableReadOnlyMemory<T> a, EquatableReadOnlyMemory<T> b) => !a.Equals(b);

    public static implicit operator EquatableReadOnlyMemory<T>(ReadOnlyMemory<T> memory) => new(memory);
    public static implicit operator ReadOnlyMemory<T>(EquatableReadOnlyMemory<T> memory) => memory.Memory;

    public static implicit operator EquatableReadOnlyMemory<T>(Memory<T> memory) => new(memory);
    public static implicit operator EquatableReadOnlyMemory<T>(T[] array) => new(array);
    public static implicit operator EquatableReadOnlyMemory<T>(ImmutableArray<T> array) => new(array.AsMemory());
    public static implicit operator EquatableReadOnlyMemory<T>(EquatableImmutableArray<T> array) => new(array.Array.AsMemory());

    public ReadOnlyMemory<T> AsMemory() => _memory;

    public EquatableReadOnlyMemory<T> Slice(int start) => new(_memory.Slice(start));
    public EquatableReadOnlyMemory<T> Slice(int start, int length) => new(_memory.Slice(start, length));
}
