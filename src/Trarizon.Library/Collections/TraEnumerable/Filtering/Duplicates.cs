namespace Trarizon.Library.Collections;
partial class TraEnumerable
{
    public static IEnumerable<T> Duplicates<T>(this IEnumerable<T> source)
        => source.DuplicatesBy(x => x);

    public static IEnumerable<T> DuplicatesBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
    {
        if (source is T[] { Length: 0 or 1 })
            return [];
        return Iterate(source, keySelector);

        static IEnumerable<T> Iterate(IEnumerable<T> source, Func<T, TKey> keySelector)
        {
            var groups = source.GroupBy(keySelector);
            foreach (var group in groups) {
                if (group.CountsMoreThan(1)) {
                    foreach (var v in group) {
                        yield return v;
                    }
                }
            }
        }
    }
}
