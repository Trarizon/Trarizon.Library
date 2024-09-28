using CommunityToolkit.Diagnostics;

namespace Trarizon.Library.Collections;
partial class TraList
{
    public readonly struct SortedModifier<T>
    {
        private readonly List<T> _list;
        private readonly IComparer<T> _comparer;

        internal SortedModifier(List<T> list, IComparer<T> comparer)
        {
            _list = list;
            _comparer = comparer;
        }

        public List<T> List => _list;

        public int Count => _list.Count;

        public T this[int index] => _list[index];

        public void Add(T item)
        {
            var index = _list.BinarySearch(item, _comparer);
            if (index >= 0) {
                _list.Insert(index, item);
            }
            else {
                _list.Insert(~index, item);
            }
        }

        public bool Remove(T item)
        {
            var index = _list.BinarySearch(item, _comparer);
            if (index >= 0) {
                _list.RemoveAt(index);
                return true;
            }
            else
                return false;
        }

        public RangeSortedModifier<T> Slice(int start, int count)
        {
            Guard.IsLessThanOrEqualTo(start + count, _list.Count);
            return new(_list, _comparer, start, count);
        }
    }

    public struct RangeSortedModifier<T>
    {
        private readonly List<T> _list;
        private readonly IComparer<T> _comparer;
        private readonly int _start;
        private int _count;

        internal RangeSortedModifier(List<T> list, IComparer<T> comparer, int start, int count)
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

        public void Add(T item)
        {
            var index = _list.BinarySearch(_start, _count, item, _comparer);
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

        public readonly RangeSortedModifier<T> Slice(int start, int count)
        {
            Guard.IsLessThanOrEqualTo(start + count, _count);
            return new(_list, _comparer, _start + start, count);
        }
    }
}
