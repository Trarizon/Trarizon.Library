namespace Trarizon.Library.GeneratorToolkit.CoreLib.Collections;
partial class TraEnumerable
{
    public static IEnumerable<(T, T)> Adjacent<T>(this IEnumerable<T> source)
    {
        if (source is T[] { Length: <= 1 })
            return [];

        return Iterate();

        IEnumerable<(T, T)> Iterate()
        {
            using var enumerator = source.GetEnumerator();
            if (!enumerator.MoveNext())
                yield break;
            T prev = enumerator.Current;

            while (enumerator.TryMoveNext(out var curr)) {
                yield return (prev, curr);
                prev = curr;
            }
        }
    }
}
