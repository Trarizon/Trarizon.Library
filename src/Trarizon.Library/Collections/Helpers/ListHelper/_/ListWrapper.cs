using System.Collections;

namespace Trarizon.Library.Collections.Helpers;
partial class ListHelper
{
    internal readonly struct ListWrapper<T> : IList<T>
    {
#nullable disable
        public readonly IReadOnlyList<T> List;
#nullable restore

        public T this[int index] { get => List[index]; set => ThrowHelper.ThrowNotSupport(ThrowConstants.CollectionIsReadOnly); }

        public int Count => List.Count;

        public bool IsReadOnly => true;

        public void Add(T item) => ThrowHelper.ThrowNotSupport(ThrowConstants.CollectionIsReadOnly);
        public void Clear() => ThrowHelper.ThrowNotSupport(ThrowConstants.CollectionIsReadOnly);
        public bool Contains(T item) => IndexOf(item) >= 0;
        public void CopyTo(T[] array, int arrayIndex)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan(arrayIndex, array.Length - Count);

            for (int i = 0; i < Count; i++)
                array[i + arrayIndex] = this[i];
        }
        public IEnumerator<T> GetEnumerator() => List.GetEnumerator();
        public int IndexOf(T item)
        {
            for (int i = 0; i < Count; i++) {
                if (EqualityComparer<T>.Default.Equals(this[i], item))
                    return i;
            }
            return -1;
        }
        public void Insert(int index, T item) => ThrowHelper.ThrowNotSupport(ThrowConstants.CollectionIsReadOnly);
        public bool Remove(T item) { ThrowHelper.ThrowNotSupport(ThrowConstants.CollectionIsReadOnly); return default; }
        public void RemoveAt(int index) => ThrowHelper.ThrowNotSupport(ThrowConstants.CollectionIsReadOnly);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}