namespace Trarizon.Library.Linq;
public static partial class TraEnumerable
{
    public static IEnumerable<T> TakeEvery<T>(this IEnumerable<T> source, int interval)
    {
        if (interval <= 1)
            return source;

        if (source.IsEmptyArray())
            return [];

        return Iterate(source, interval);

        static IEnumerable<T> Iterate(IEnumerable<T> source, int interval)
        {
            using var enumerator = source.GetEnumerator();

            while (enumerator.MoveNext()) {
                yield return enumerator.Current;
                for (int i = 1; i < interval; i++) {
                    if (!enumerator.MoveNext())
                        yield break;
                }
            }
        }
    }
}
