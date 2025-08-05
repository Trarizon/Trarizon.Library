using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.InteropServices;

namespace Trarizon.Library.Roslyn.Collections;
public readonly struct SequenceEquatableImmutableArray<T>(ImmutableArray<T> array)
    : IEquatable<SequenceEquatableImmutableArray<T>>
    , IReadOnlyCollection<T>
{
    public ImmutableArray<T> Array { get; } = array;

    public int Count => ((IReadOnlyCollection<T>)Array).Count;

    public bool Equals(SequenceEquatableImmutableArray<T> other)
    {
        if (ReferenceEquals(ImmutableCollectionsMarshal.AsArray(Array), ImmutableCollectionsMarshal.AsArray(other.Array)))
            return true;
        if (Array.Length != other.Array.Length)
            return false;

        for (int i = 0; i < Array.Length; i++) {
            if (!EqualityComparer<T>.Default.Equals(Array[i], other.Array[i]))
                return false;
        }
        return true;
    }

    public override bool Equals(object obj) => obj is SequenceEquatableImmutableArray<T> other && Equals(other);

    public override int GetHashCode()
    {
        HashCode hc = new();
        for (int i = 0; i < Array.Length; i++) {
            hc.Add(Array[i]);
        }
        return hc.ToHashCode();
    }

    public static bool operator ==(SequenceEquatableImmutableArray<T> left, SequenceEquatableImmutableArray<T> right) => left.Equals(right);
    public static bool operator !=(SequenceEquatableImmutableArray<T> left, SequenceEquatableImmutableArray<T> right) => !(left == right);

    public ImmutableArray<T>.Enumerator GetEnumerator() => Array.GetEnumerator();
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => ((IEnumerable<T>)Array).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Array).GetEnumerator();
}
