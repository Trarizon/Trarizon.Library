namespace Trarizon.Library.Collections.Helpers.Queriers;
internal abstract class ListQuerier<T> : EnumerationQuerier<T>, IList<T>, IReadOnlyList<T>
{
    public abstract T this[int index] { get; }
    public abstract int Count { get; }

    public bool IsReadOnly => false;

    protected ListQuerier() { }

    protected virtual void SetAt(int index, T item) => ThrowHelper.ThrowNotSupport(ThrowConstants.QuerierImmutable);

    T IList<T>.this[int index] { get => this[index]; set => SetAt(index, value); }

    int IList<T>.IndexOf(T item)
    {
        for (int i = 0; i < Count; i++) {
            if (EqualityComparer<T>.Default.Equals(this[i], item))
                return i;
        }
        return -1;
    }
    void IList<T>.Insert(int index, T item) => ThrowHelper.ThrowNotSupport(ThrowConstants.QuerierImmutable);
    void IList<T>.RemoveAt(int index) => ThrowHelper.ThrowNotSupport(ThrowConstants.QuerierImmutable);
    void ICollection<T>.Add(T item) => ThrowHelper.ThrowNotSupport(ThrowConstants.QuerierImmutable);
    void ICollection<T>.Clear() => ThrowHelper.ThrowNotSupport(ThrowConstants.QuerierImmutable);
    bool ICollection<T>.Contains(T item) => ((IList<T>)this).IndexOf(item) >= 0;
    void ICollection<T>.CopyTo(T[] array, int arrayIndex)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(arrayIndex, array.Length - Count);

        for (int i = 0; i < array.Length; i++)
            array[i + arrayIndex] = this[i];
    }
    bool ICollection<T>.Remove(T item) { ThrowHelper.ThrowNotSupport(ThrowConstants.QuerierImmutable); return default; }
}
