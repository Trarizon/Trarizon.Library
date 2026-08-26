using System.Collections;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Trarizon.Library.Roslyn.Pipeline;

public static class CollectionBuilders
{
    public static EquatableImmutableArray<T> CreateEquatableImmutableArray<T>(ReadOnlySpan<T> items)
        => new(items.ToImmutableArray());
}

[CollectionBuilder(typeof(CollectionBuilders), nameof(CollectionBuilders.CreateEquatableImmutableArray))]
public readonly struct EquatableImmutableArray<T>(ImmutableArray<T> array)
    : IEquatable<EquatableImmutableArray<T>>
    , IReadOnlyCollection<T>
    , IReadOnlyList<T>
{
    private readonly ImmutableArray<T> _array = array;

    public ImmutableArray<T> Array => _array.IsDefault ? ImmutableArray<T>.Empty : _array;

    public int Length => Array.Length;

    public T this[int index] => Array[index];

    public bool Equals(EquatableImmutableArray<T> other)
    {
#if IMMUTABLE_MARSHAL
        if (ReferenceEquals(ImmutableCollectionsMarshal.AsArray(Array), ImmutableCollectionsMarshal.AsArray(other.Array)))
            return true;
#endif
        if (Array.Length != other.Array.Length)
            return false;

        for (int i = 0; i < Array.Length; i++)
        {
            if (!EqualityComparer<T>.Default.Equals(Array[i], other.Array[i]))
                return false;
        }
        return true;
    }

    public override bool Equals(object obj) => obj is EquatableImmutableArray<T> other && Equals(other);

    public override int GetHashCode()
    {
        HashCode hc = new();
        for (int i = 0; i < Array.Length; i++)
        {
            hc.Add(Array[i]);
        }
        return hc.ToHashCode();
    }

    public static bool operator ==(EquatableImmutableArray<T> left, EquatableImmutableArray<T> right) => left.Equals(right);
    public static bool operator !=(EquatableImmutableArray<T> left, EquatableImmutableArray<T> right) => !(left == right);

    public static implicit operator EquatableImmutableArray<T>(ImmutableArray<T> array) => new(array);
    public static implicit operator ImmutableArray<T>(EquatableImmutableArray<T> array) => array.Array;

    public static ImmutableArray<T> AsImmutableArray(EquatableImmutableArray<T> array) => array.Array;

    public ImmutableArray<T>.Enumerator GetEnumerator() => Array.GetEnumerator();
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => ((IEnumerable<T>)Array).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Array).GetEnumerator();

    int IReadOnlyCollection<T>.Count => Length;
}
