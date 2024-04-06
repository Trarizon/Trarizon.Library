namespace Trarizon.Library.Collections.Helpers;
partial class EnumerableQuery
{
    public static List<T>? ToListIfAny<T>(this IEnumerable<T> source)
    {
        using var enumerator = source.GetEnumerator();
        if (!enumerator.MoveNext())
            return null;

        List<T> list;
        if (source.TryGetNonEnumeratedCount(out var count)) 
            list = new List<T>(count) { enumerator.Current };
        else 
            list = [enumerator.Current];

        while (enumerator.MoveNext()) {
            list.Add(enumerator.Current);
        }
        return list;
    }
}
