using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Trarizon.Library.Roslyn.Collections;
public readonly struct SequenceEquatableCollection<TCollection, T>(TCollection collection)
    : IEquatable<SequenceEquatableCollection<TCollection, T>>
    , IEnumerable<T>
    where TCollection : IEnumerable<T>
{
    public TCollection Collection { get; } = collection;

    public bool Equals(SequenceEquatableCollection<TCollection, T> other)
    {
        if (ReferenceEquals(Collection, other.Collection))
            return true;
        return Collection.SequenceEqual(other.Collection);
    }

    public override bool Equals(object obj) => obj is SequenceEquatableCollection<TCollection, T> other && Equals(other);

    public IEnumerator<T> GetEnumerator() => Collection.GetEnumerator();

    public override int GetHashCode()
    {
        HashCode hc = new();
        foreach (var item in Collection) {
            hc.Add(item);
        }
        return hc.ToHashCode();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public static bool operator ==(SequenceEquatableCollection<TCollection, T> left, SequenceEquatableCollection<TCollection, T> right) => left.Equals(right);
    public static bool operator !=(SequenceEquatableCollection<TCollection, T> left, SequenceEquatableCollection<TCollection, T> right) => !(left == right);

    public static implicit operator SequenceEquatableCollection<TCollection, T>(TCollection collection) => new(collection);
}
