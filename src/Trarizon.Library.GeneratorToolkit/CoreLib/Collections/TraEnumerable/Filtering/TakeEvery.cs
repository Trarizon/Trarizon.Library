namespace Trarizon.Library.GeneratorToolkit.CoreLib.Collections;
partial class TraEnumerable
{
    public static IEnumerable<T> TakeEvery<T>(this IEnumerable<T> source, int interval)
    {
        if (interval <= 1)
            return source;

        if (source.IsEmptyArray())
            return [];

        return Iterate();

        IEnumerable<T> Iterate()
        {
            int count = 0;
            using var enumerator = source.GetEnumerator();

            while (enumerator.MoveNext()) {
                yield return enumerator.Current;
                for (int i = 1; i < count; i++) {
                    if (!enumerator.MoveNext())
                        yield break;
                }
            }
        }
    }
}
