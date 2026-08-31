using System.Collections;

namespace Trarizon.Library.Roslyn.Pipeline;

public readonly struct EquatableCollection<TCollection, T>(TCollection collection)
    : IEquatable<EquatableCollection<TCollection, T>>
    , IEnumerable<T>
    where TCollection : IEnumerable<T>
{
    public TCollection Collection { get; } = collection;

    public bool Equals(EquatableCollection<TCollection, T> other)
    {
        if (ReferenceEquals(Collection, other.Collection))
            return true;
        return Collection.SequenceEqual(other.Collection);
    }

    public override bool Equals(object obj) => obj is EquatableCollection<TCollection, T> other && Equals(other);

    public IEnumerator<T> GetEnumerator() => Collection.GetEnumerator();

    public override int GetHashCode()
    {
        HashCode hc = new();
        foreach (var item in Collection)
        {
            hc.Add(item);
        }
        return hc.ToHashCode();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public static bool operator ==(EquatableCollection<TCollection, T> left, EquatableCollection<TCollection, T> right) => left.Equals(right);
    public static bool operator !=(EquatableCollection<TCollection, T> left, EquatableCollection<TCollection, T> right) => !(left == right);

    public static implicit operator EquatableCollection<TCollection, T>(TCollection collection) => new(collection);
}
