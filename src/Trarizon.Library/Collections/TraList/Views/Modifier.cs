using CommunityToolkit.Diagnostics;
using CommunityToolkit.HighPerformance;

namespace Trarizon.Library.Collections;
partial class TraList
{
    /// <summary>
    /// Returns a view through which modifying the list will keep elements in order.
    /// <br/>
    /// Make sure your list is in order
    /// </summary>
    public static SortedModifier<T, Comparer<T>> GetSortedModifier<T>(this List<T> list)
        => GetSortedModifier(list, Comparer<T>.Default);

    /// <summary>
    /// Returns a view through which modifying the list will keep elements in order.
    /// <br/>
    /// Make sure your list is in order
    /// </summary>
    public static SortedModifier<T, TComparer> GetSortedModifier<T, TComparer>(this List<T> list, TComparer comparer) where TComparer : IComparer<T>
        => new(list, comparer);

    /// <summary>
    /// Returns a view through which modifying the list will keep elements in order.
    /// <br/>
    /// Make sure your list is in order
    /// </summary>
    public static RangeSortedModifier<T, Comparer<T>> GetSortedModifier<T>(this List<T> list, int start, int count)
        => new(list, Comparer<T>.Default, start, count);

    /// <summary>
    /// Returns a view through which modifying the list will keep elements in order.
    /// <br/>
    /// Make sure your list is in order
    /// </summary>
    public static RangeSortedModifier<T, TComparer> GetSortedModifier<T, TComparer>(this List<T> list, int start, int count, TComparer comparer) where TComparer : IComparer<T>
        => new(list, comparer, start, count);

    public readonly struct SortedModifier<T, TComparer> where TComparer : IComparer<T>
    {
        private readonly List<T> _list;
        private readonly TComparer _comparer;

        internal SortedModifier(List<T> list, TComparer comparer)
        {
            _list = list;
            _comparer = comparer;
        }

        public List<T> List => _list;

        public int Count => _list.Count;

        public T this[int index] => _list[index];

        /// <summary>
        /// Search for the right index to add and insert <paramref name="item"/> into collection
        /// </summary>
        public void Add(T item)
        {
            var index = _list.BinarySearch(item, _comparer);
            if (index < 0)
                index = ~index;
            _list.Insert(index, item);
        }

        /// <summary>
        /// Search for add index from the end of collection, and insert <paramref name="item"/>
        /// </summary>
        public void AddFromEnd(T item)
        {
            var index = _list.AsSpan().LinearSearchFromEnd(item, _comparer);
            if (index < 0)
                index = ~index;
            _list.Insert(index, item);
        }

        /// <summary>
        /// Search for add index from the start of collection, and insert <paramref name="item"/>
        /// </summary>
        public void AddFromStart(T item)
        {
            var index = _list.AsSpan().LinearSearch(item, _comparer);
            if (index < 0)
                index = ~index;
            _list.Insert(index, item);
        }

        /// <summary>
        /// Remove <paramref name="item"/> in collection if found, and returns the
        /// original index of the removed item in collection
        /// </summary>
        /// <returns>
        /// The original index of <paramref name="item"/> in collection if removed,
        /// else return bitwise complement of index of the next element that larger than <paramref name="item"/>
        /// </returns>
        public int Remove(T item)
        {
            var index = _list.BinarySearch(item, _comparer);
            if (index >= 0) {
                _list.RemoveAt(index);
            }
            return index;
        }

        public RangeSortedModifier<T, TComparer> Slice(int start, int count)
        {
            Guard.IsLessThanOrEqualTo(start + count, _list.Count);
            return new(_list, _comparer, start, count);
        }
    }

    public struct RangeSortedModifier<T, TComparer> where TComparer : IComparer<T>
    {
        private readonly List<T> _list;
        private readonly TComparer _comparer;
        private readonly int _start;
        private int _count;

        internal RangeSortedModifier(List<T> list, TComparer comparer, int start, int count)
        {
            _list = list;
            _comparer = comparer;
            _start = start;
            _count = count;
        }

        public readonly List<T> List => _list;

        public readonly int Count => _count;

        public readonly T this[int index]
        {
            get {
                Guard.IsInRange(index, 0, _count);
                return _list[index + _start];
            }
        }

        /// <summary>
        /// Search for the right index to add and insert <paramref name="item"/> into collection
        /// </summary>
        public void Add(T item)
        {
            var index = _list.BinarySearch(_start, _count, item, _comparer);
            if (index < 0)
                index = ~index;
            _list.Insert(index, item);
            _count++;
        }

        /// <summary>
        /// Search for add index from the end of collection, and insert <paramref name="item"/>
        /// </summary>
        public void AddFromEnd(T item)
        {
            var index = _list.AsSpan().LinearSearchFromEnd(item, _comparer);
            if (index < 0)
                index = ~index;
            _list.Insert(index, item);
            _count++;
        }

        /// <summary>
        /// Search for add index from the start of collection, and insert <paramref name="item"/>
        /// </summary>
        public void AddFromStart(T item)
        {
            var index = _list.AsSpan().LinearSearch(item, _comparer);
            if (index < 0)
                index = ~index;
            _list.Insert(index, item);
            _count++;
        }

        public bool Remove(T item)
        {
            var index = _list.BinarySearch(_start, _count, item, _comparer);
            if (index >= 0) {
                _list.RemoveAt(index);
                _count--;
                return true;
            }
            else
                return false;
        }

        public readonly RangeSortedModifier<T, TComparer> Slice(int start, int count)
        {
            Guard.IsLessThanOrEqualTo(start + count, _count);
            return new(_list, _comparer, _start + start, count);
        }
    }
}
