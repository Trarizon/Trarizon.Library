using CommunityToolkit.HighPerformance;
using Trarizon.Library.Common;

namespace Trarizon.Library.Collections;
public static partial class TraCollection
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

        public T this[int index]
        {
            get => _list[index];
            set {
                _list[index] = value;
                NotifyEdited(index);
            }
        }

        #region Search  

        public int BinarySearch(T item) => _list.BinarySearch(item, _comparer);

        public int BinarySearch(Range priorRange, T item)
            => TraSpan.BinarySearchRangePriority(_list.AsSpan(), priorRange, new TraComparison.ComparerComparable<T, TComparer>(item, _comparer));

        public int LinearSearch(T item) => _list.AsSpan().LinearSearch(item, _comparer);

        public int LinearSearchFromEnd(T item) => _list.AsSpan().LinearSearchFromEnd(item, _comparer);

        public int LinearSearch(Index nearIndex, T item)
            => TraSpan.LinearSearchFromNear(_list.AsSpan(), nearIndex.GetOffset(Count), new TraComparison.ComparerComparable<T, TComparer>(item, _comparer));

        public bool Contains(T item) => BinarySearch(item) >= 0;

        #endregion

        #region Add

        /// <summary>
        /// Insert <paramref name="item"/> into collection with items keep in order
        /// </summary>
        public void Add(T item)
        {
            var index = _list.BinarySearch(item, _comparer);
            TraNumber.FlipNegative(ref index);
            _list.Insert(index, item);
        }

        public void Add(Index nearIndex, T item)
        {
            var index = LinearSearch(nearIndex, item);
            TraNumber.FlipNegative(ref index);
            _list.Insert(index, item);
        }

        public void Add(Range priorRange, T item)
        {
            var index = BinarySearch(priorRange, item);
            TraNumber.FlipNegative(ref index);
            _list.Insert(index, item);
        }

        #endregion

        #region Remove

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

        public int Remove(T item, Index nearIndex)
        {
            var index = LinearSearch(nearIndex, item);
            if (index >= 0)
                _list.RemoveAt(index);
            return index;
        }

        public int Remove(T item, Range guessRange)
        {
            var index = BinarySearch(guessRange, item);
            if (index >= 0)
                _list.RemoveAt(index);
            return index;
        }

        #endregion

        public void Resort() => _list.Sort(_comparer);

        public void ResortBubble() => TraAlgorithm.BubbleSort(_list.AsSpan(), _comparer);

        /// <summary>
        /// Notify that the item at <paramref name="index"/> is edited,
        /// the method will move the item to the correct position to keep list in order
        /// </summary>
        /// <param name="index"></param>
        public void NotifyEdited(int index)
        {
            var editItem = _list[index];
            if (index >= 1 && _comparer.Compare(_list[index - 1], editItem) > 0) {
                var destIndex = _list.BinarySearch(0, index, editItem, _comparer);
                if (destIndex < 0)
                    destIndex = ~destIndex;
                _list.AsSpan().MoveTo(index, destIndex);
            }
            else if (index < _list.Count - 1 && _comparer.Compare(editItem, _list[index + 1]) > 0) {
                var destIndex = _list.BinarySearch(index + 1, _list.Count - 1 - index, editItem, _comparer);
                if (destIndex < 0)
                    destIndex = ~destIndex;
                _list.AsSpan().MoveTo(index, index + destIndex);
            }
            else {
                // No move
            }
        }
    }
}
