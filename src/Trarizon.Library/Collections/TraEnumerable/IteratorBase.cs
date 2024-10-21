using CommunityToolkit.Diagnostics;
using System.Collections;

namespace Trarizon.Library.Collections;
partial class TraEnumerable
{
    private abstract class IteratorBase<T> : IEnumerable<T>, IEnumerator<T>
    {
        protected const int MinPreservedState = -2;
        protected const int InitState = -1;

        protected int _state = -2;
        private readonly int _threadId = Environment.CurrentManagedThreadId;

        public abstract T Current { get; }
        protected abstract IteratorBase<T> Clone();
        public abstract bool MoveNext();
        protected virtual void DisposeInternal() { }
        public virtual void Reset() => ThrowHelper.ThrowNotSupportedException();

        public IEnumerator<T> GetEnumerator()
        {
            var rtn = _state == -2 && _threadId == Environment.CurrentManagedThreadId ? this : Clone();
            rtn._state = -1;
            return rtn;
        }

        public void Dispose()
        {
            DisposeInternal();
            GC.SuppressFinalize(this);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

#nullable disable
        object IEnumerator.Current => Current;
#nullable restore
    }

    private abstract class ListIteratorBase<T> : IteratorBase<T>, IList<T>, IReadOnlyList<T>
    {
        public abstract T this[int index] { get; }

        T IList<T>.this[int index] { get => this[index]; set => TraThrow.IteratorImmutable(); }

        public abstract int Count { get; }

        public int IndexOf(T item)
        {
            if (typeof(T).IsValueType) {
                for (int i = 0; i < Count; i++) {
                    if (EqualityComparer<T>.Default.Equals(this[i], item))
                        return i;
                }
                return -1;
            }
            else {
                var comparer = EqualityComparer<T>.Default;
                for (int i = 0; i < Count; i++) {
                    if (comparer.Equals(this[i], item))
                        return i;
                }
                return -1;
            }
        }

        public bool Contains(T item) => IndexOf(item) != -1;

        public void CopyTo(T[] array, int arrayIndex)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(arrayIndex);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(arrayIndex, array.Length - Count);

            for (int i = 0; i < Count; i++) {
                array[arrayIndex + i] = this[i];
            }
        }
      
        bool ICollection<T>.IsReadOnly => true;
        void ICollection<T>.Add(T item) => TraThrow.IteratorImmutable();
        void ICollection<T>.Clear() => TraThrow.IteratorImmutable();
        void IList<T>.Insert(int index, T item) => TraThrow.IteratorImmutable();
        bool ICollection<T>.Remove(T item) { TraThrow.IteratorImmutable(); return default; }
        void IList<T>.RemoveAt(int index) => TraThrow.IteratorImmutable();
    }
}
