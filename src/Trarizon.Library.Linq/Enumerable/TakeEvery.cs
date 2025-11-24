namespace Trarizon.Library.Linq;

public static partial class TraEnumerable
{
    public static IEnumerable<T> TakeEvery<T>(this IEnumerable<T> source, int step)
    {
        if (step <= 1)
            return source;

        if (source is T[] arr) {
            return arr.Length switch
            {
                0 => [],
                1 => arr,
                _ => new ListTakeEveryIterator<T>(arr, step),
            };
        }

        if (source is IList<T> list) {
            return new ListTakeEveryIterator<T>(list, step);
        }

        return Iterate(source, step);

        static IEnumerable<T> Iterate(IEnumerable<T> source, int step)
        {
            using var enumerator = source.GetEnumerator();

            while (enumerator.MoveNext()) {
                yield return enumerator.Current;
                for (int i = 1; i < step; i++) {
                    if (!enumerator.MoveNext())
                        yield break;
                }
            }
        }
    }

    private sealed class ListTakeEveryIterator<T>(IList<T> list, int step) : ListIteratorBase<T>
    {
        private T? _current;

        public override T this[int index] => list[index * 3];

        public override int Count => list.Count / 3 + 1;

        public override T Current => _current!;

        public override bool MoveNext() => MoveNext_Index(ref _current);

        protected override IteratorBase<T> Clone() => new ListTakeEveryIterator<T>(list, step);
    }
}
