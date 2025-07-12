using System.Collections;
using System.Diagnostics;
using Trarizon.Library.Collections.Helpers;

namespace Trarizon.Library.Collections;
public static partial class TraEnumerable
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
        public virtual void Reset() => Throws.IteratorNotSupport();

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

    private abstract class CollectionIteratorBase<T> : IteratorBase<T>, ICollection<T>, IReadOnlyCollection<T>, ICollection
    {
        public abstract int Count { get; }

        public virtual bool Contains(T item)
        {
            if (typeof(T).IsValueType) {
                foreach (var val in this) {
                    if (EqualityComparer<T>.Default.Equals(val, item))
                        return true;
                }
                return false;
            }
            else {
                var comparer = EqualityComparer<T>.Default;
                foreach (var val in this) {
                    if (comparer.Equals(val, item))
                        return true;
                }
                return false;
            }
        }

        public virtual void CopyTo(T[] array, int arrayIndex)
        {
            int count = Count;
            Throws.ThrowIfNegative(arrayIndex);
            Throws.ThrowIfGreaterThan(arrayIndex, array.Length - count);

            var span = array.AsSpan(arrayIndex, count);
            int i = 0;
            foreach (var val in this) {
                span[i++] = val;
            }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            Throws.ThrowIfNegative(index);
            Throws.ThrowIfGreaterThan(index, array.Length - Count);

            if (array.Rank != 1)
                Throws.ThrowInvalidOperation("Array must have rank 1");

            foreach (var val in this) {
                array.SetValue(val, index++);
            }
        }

        bool ICollection.IsSynchronized => true;
        object ICollection.SyncRoot => this;
        bool ICollection<T>.IsReadOnly => true;
        void ICollection<T>.Add(T item) => Throws.IteratorNotSupport();
        void ICollection<T>.Clear() => Throws.IteratorNotSupport();
        bool ICollection<T>.Remove(T item) { Throws.IteratorNotSupport(); return default; }
    }

    private abstract class ListIteratorBase<T> : CollectionIteratorBase<T>, IList<T>, IReadOnlyList<T>
    {
        public abstract T this[int index] { get; }

        T IList<T>.this[int index] { get => this[index]; set => Throws.IteratorNotSupport(); }

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

        public sealed override bool Contains(T item) => IndexOf(item) != -1;

        void IList<T>.Insert(int index, T item) => Throws.IteratorNotSupport();
        void IList<T>.RemoveAt(int index) => Throws.IteratorNotSupport();

        protected bool MoveNext_Index(ref T? currentField)
        {
            const int End = MinPreservedState - 1;

            switch (_state) {
                case InitState:
                    _state = 0;
                    goto default;
                case End:
                    return false;
                default:
                    Debug.Assert(_state >= 0);
                    var index = _state;
                    if (index < Count) {
                        currentField = this[_state];
                        _state = index + 1;
                        return true;
                    }
                    currentField = default;
                    _state = End;
                    return false;
            }
        }
    }
}
