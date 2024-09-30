namespace Trarizon.Library.GeneratorToolkit.CoreLib.Collections;
partial class TraEnumerable
{
    public static IEnumerable<(T, T?)> ChunkPair<T>(this IEnumerable<T> source)
    {
        if (source.IsEmptyArray())
            return [];
        return Iterate();

        IEnumerable<(T, T?)> Iterate()
        {
            using var enumerator = source.GetEnumerator();

            while (enumerator.TryMoveNext(out var left)) {
                if (enumerator.TryMoveNext(out var right))
                    yield return (left, right);
                else {
                    yield return (left, default);
                    yield break;
                }
            }
        }
    }

    public static IEnumerable<(T, T?, T?)> ChunkTriple<T>(this IEnumerable<T> source)
    {
        if (source.IsEmptyArray())
            return [];
        return Iterate();

        IEnumerable<(T, T?, T?)> Iterate()
        {
            using var enumerator = source.GetEnumerator();

            while (enumerator.TryMoveNext(out var first)) {
                if (!enumerator.TryMoveNext(out var second)) {
                    yield return (first, default, default);
                    yield break;
                }

                if(!enumerator.TryMoveNext(out var third)) {
                    yield return (first, second, default);
                    yield break;
                }

                yield return (first, second, third);
            }
        }
    }
}
