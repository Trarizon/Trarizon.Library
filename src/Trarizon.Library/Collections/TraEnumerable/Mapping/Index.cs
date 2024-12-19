namespace Trarizon.Library.Collections;
partial class TraEnumerable
{
    public static IEnumerable<(int Index, T Item)> WithIndex<T>(this IEnumerable<T> source)
    {
        if (source is T[] array)
            return array.Length == 0 ? [] : new ListWithIndexIterator<T>(array);
        if (source.IsEmptyArray())
            return [];
        return Iterate(source);

        static IEnumerable<(int, T)> Iterate(IEnumerable<T> source)
        {
            int i = 0;
            foreach (var item in source) {
                yield return (i++, item);
            }
        }
    }

    private sealed class ListWithIndexIterator<T>(T[] array) : ListIteratorBase<(int, T)>
    {
        public override (int, T) this[int index] => (index, array[index]);

        public override int Count => array.Length;

        private (int, T) _current;
        public override (int, T) Current => _current;

        public override bool MoveNext() => MoveNext_Array(array, ref _current);

        protected override IteratorBase<(int, T)> Clone() => new ListWithIndexIterator<T>(array);
    }
}
