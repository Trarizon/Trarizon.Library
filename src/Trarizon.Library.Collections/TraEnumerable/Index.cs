namespace Trarizon.Library.Collections;
public static partial class TraEnumerable
{
#if !NET9_OR_GREATER

    public static IEnumerable<(int Index, T Item)> Index<T>(this IEnumerable<T> source)
    {
        if (source is IList<T> list) {
            if (source.IsEmptyArray())
                return [];
            return new ListWithIndexIterator<T>(list);
        }
        return Iterate(source);

        static IEnumerable<(int, T)> Iterate(IEnumerable<T> source)
        {
            int i = 0;
            foreach (var item in source) {
                yield return (i++, item);
            }
        }
    }

    private sealed class ListWithIndexIterator<T>(IList<T> array) : ListIteratorBase<(int, T)>
    {
        public override (int, T) this[int index] => (index, array[index]);

        public override int Count => array.Count;

        private (int, T) _current;
        public override (int, T) Current => _current;

        public override bool MoveNext() => MoveNext_Index(ref _current);

        protected override IteratorBase<(int, T)> Clone() => new ListWithIndexIterator<T>(array);

        public override bool Contains((int, T) value)
        {
            return EqualityComparer<T>.Default.Equals(array[value.Item1], value.Item2);
        }

        public override int IndexOf((int, T) item)
        {
            if (Contains(item))
                return item.Item1;
            return -1;
        }
    }

#endif
}
