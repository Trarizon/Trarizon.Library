namespace Trarizon.Library.Collections;
partial class TraEnumerable
{
    /// <summary>
    /// If <paramref name="source"/> is <see langword="null"/>, this method returns an empty collection,
    /// else return <paramref name="source"/> it self
    /// </summary>
    public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T>? source)
        => source ?? [];

    public static List<T>? ToNonEmptyListOrNull<T>(this IEnumerable<T> source)
    {
        if (source.TryGetNonEnumeratedCount(out var count)) {
            if (count == 0)
                return null;
            else
                return source.ToList();
        }

        using var enumerator = source.GetEnumerator();
        if (!enumerator.MoveNext())
            return null;

        List<T> list = [enumerator.Current];
        while (enumerator.MoveNext()) {
            list.Add(enumerator.Current);
        }
        return list;
    }
}
