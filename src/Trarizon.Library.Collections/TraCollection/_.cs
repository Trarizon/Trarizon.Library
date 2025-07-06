using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Trarizon.Library.Collections.Helpers;

namespace Trarizon.Library.Collections;
public static partial class TraCollection
{
    public static ReadOnlySingletonCollection<T> Singleton<T>(T value) => new ReadOnlySingletonCollection<T>(value);

    internal static bool TryGetSpan<T>(this IEnumerable<T> source, out ReadOnlySpan<T> span)
    {
        if (source.GetType() == typeof(T[])) {
            span = Unsafe.As<T[]>(source).AsSpan();
            return true;
        }
        if (source.GetType() == typeof(List<T>)) {
            var list = Unsafe.As<List<T>>(source);
#if NET8_0_OR_GREATER
            span = CollectionsMarshal.AsSpan(list);
#else
            span = Utils<T>.GetUnderlyingArray(list);
#endif
            return true;
        }
        span = default;
        return false;
    }

    public static class Utils<T>
    {
        #region List

#if NET9_0_OR_GREATER
        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_version")]
        public static extern ref int GetVersion(List<T> list);

        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_items")]
        public static extern ref T[] GetUnderlyingArray(List<T> list);
#else
        public static ref T[] GetUnderlyingArray(List<T> list)
            => ref Unsafe.As<List<T>, StrongBox<T[]>>(ref list).Value!;
#endif

        #endregion

        #region Stack

#if NET9_0_OR_GREATER
        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_array")]
        public static extern ref T[] GetUnderlyingArray(Stack<T> stack);
#else
        public static ref T[] GetUnderlyingArray(Stack<T> stack)
            => ref Unsafe.As<StackMarchalHelper>(stack)._array!;

        private class StackMarchalHelper
        {
#pragma warning disable CS0649
#nullable disable
            public T[] _array;
            public int _size;
            public int _version;
#nullable restore
#pragma warning restore CS0649
        }
#endif

        #endregion
    }

    public readonly struct ReadOnlySingletonCollection<T>(T value) : IList<T>, IReadOnlyList<T>, ICollection<T>, IReadOnlyCollection<T>
    {
        private readonly T _value = value;

        public T Value => _value;

        public Enumerator GetEnumerator() => new(_value);

        int ICollection<T>.Count => 1;
        int IReadOnlyCollection<T>.Count => 1;
        bool ICollection<T>.IsReadOnly => true;
        T IReadOnlyList<T>.this[int index]
        {
            get {
                if (index == 0)
                    return _value;
                else {
                    Throws.IndexArgOutOfRange(index);
                    return default!;
                }
            }
        }

        T IList<T>.this[int index]
        {
            get {
                if (index == 0)
                    return _value;
                else {
                    Throws.IndexArgOutOfRange(index);
                    return default!;
                }
            }
            set => Throws.CollectionIsReadOnly();
        }
        void ICollection<T>.Add(T item) => Throws.CollectionIsReadOnly();
        void ICollection<T>.Clear() => Throws.CollectionIsReadOnly();
        bool ICollection<T>.Contains(T item) => EqualityComparer<T>.Default.Equals(item, _value);
        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            Throws.ThrowIfIndexGreaterThanOrEqual(arrayIndex, array.Length);
            array[arrayIndex] = _value;
        }
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
        int IList<T>.IndexOf(T item) => EqualityComparer<T>.Default.Equals(item, _value) ? 0 : -1;
        void IList<T>.Insert(int index, T item) => Throws.CollectionIsReadOnly();
        bool ICollection<T>.Remove(T item) { Throws.CollectionIsReadOnly(); return default; }
        void IList<T>.RemoveAt(int index) => Throws.CollectionIsReadOnly();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public struct Enumerator : IEnumerator<T>
        {
            private readonly T _value;
            private bool _disposed;

            internal Enumerator(T value)
            {
                _value = value;
            }

            public readonly T Current => _value;

            object IEnumerator.Current => Current!;

            void IDisposable.Dispose() { }
            public bool MoveNext()
            {
                if (_disposed)
                    return false;
                else {
                    _disposed = true;
                    return true;
                }
            }
            public void Reset() => _disposed = false;
        }
    }
}
