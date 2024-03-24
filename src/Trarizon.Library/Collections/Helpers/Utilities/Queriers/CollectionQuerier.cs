namespace Trarizon.Library.Collections.Helpers.Utilities.Queriers;
internal abstract class CollectionQuerier<T> : EnumerationQuerier<T>, ICollection<T>, IReadOnlyCollection<T>
{
    public abstract int Count { get; }

    bool ICollection<T>.IsReadOnly => true;

    void ICollection<T>.Add(T item) => ThrowHelper.ThrowNotSupport(ThrowConstants.QuerierImmutable);
    void ICollection<T>.Clear() => ThrowHelper.ThrowNotSupport(ThrowConstants.QuerierImmutable);
    bool ICollection<T>.Contains(T item)
    {
        foreach (var v in this) {
            if (EqualityComparer<T>.Default.Equals(v, item))
                return true;
        }
        return false;
    }
    void ICollection<T>.CopyTo(T[] array, int arrayIndex)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(arrayIndex, array.Length - Count);

        foreach (var item in this) {
            array[arrayIndex++] = item;
        }
    }
    bool ICollection<T>.Remove(T item) => throw new NotImplementedException();
}
