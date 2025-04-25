namespace Trarizon.Library.Collections;
public static partial class TraEnumerable
{
    public static IEnumerable<T> LazyReverse<T>(this IEnumerable<T> source)
    {
        if (source is IList<T> list) {
            if (list.Count <= 1)
                return source;
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
    }
}
