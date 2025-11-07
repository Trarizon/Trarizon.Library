using System.Diagnostics;

namespace Trarizon.Library.Linq;

public static partial class TraEnumerable
{
    public static IEnumerable<T> LazyReverse<T>(this IEnumerable<T> source)
    {
        if (source is IList<T> list) {
            if (list.Count <= 1)
                return source;
            if (source is ListLazyReverseIterator<T> iterator)
                return iterator.Reverse();
            return new ListLazyReverseIterator<T>(list);
        }

        return source.Reverse();
    }

    private sealed class ListLazyReverseIterator<T>(IList<T> list) : ListIteratorBase<T>
    {
        private T? _current;

        public override T this[int index] => list[list.Count - index - 1];

        public override int Count => list.Count;

        public override T Current => _current!;

        public override bool MoveNext() => MoveNext_Index(ref _current);

        protected override IteratorBase<T> Clone() => new ListLazyReverseIterator<T>(list);

        public IList<T> Reverse() => list;

        public override bool Contains(T item) => list.Contains(item);

        public override int IndexOf(T item)
        {
            var index = list.IndexOf(item);
            return index == -1 ? -1 : list.Count - index - 1;
        }

        internal override T TryGetFirst(out bool exists)
        {
            Debug.Assert(list.Count > 0);
            exists = true;
            return list[^1];
        }

        internal override T TryGetLast(out bool exists)
        {
            Debug.Assert(list.Count > 0);
            exists = true;
            return list[0];
        }
    }

}
