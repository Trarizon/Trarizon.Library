namespace Trarizon.Library.Collections;
public static partial class TraEnumerable
{
    public static IEnumerable<(T, T?)> ChunkPair<T>(this IEnumerable<T> source)
    {
        if (source is T[] array)
            return array.Length == 0 ? [] : IterateArray(array);
        return Iterate(source);

        static IEnumerable<(T, T?)> IterateArray(T[] array)
        {
            if (array.Length % 2 == 0) {
                for (int i = 0; i < array.Length; i += 2)
                    yield return (array[i], array[i + 1]);
            }
            else {
                var len = array.Length - 1;
                for (int i = 0; i < len; i += 2)
                    yield return (array[i], array[i + 1]);
                yield return (array[^1], default);
            }
        }

        static IEnumerable<(T, T?)> Iterate(IEnumerable<T> source)
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
        if (source is T[] array)
            return array.Length == 0 ? [] : IterateArray(array);
        if (source.IsEmptyArray())
            return [];
        return Iterate(source);

        static IEnumerable<(T, T?, T?)> IterateArray(T[] array)
        {
            var rem = array.Length % 3;
            if (rem == 0) {
                for (int i = 0; i < array.Length; i += 3)
                    yield return (array[i], array[i + 1], array[i + 2]);
            }
            else {
                var len = array.Length - rem;
                for (int i = 0; i < len; i += 3)
                    yield return (array[i], array[i + 1], array[i + 2]);
                yield return (array[len], array.ElementAtOrDefault(len + 1), array.ElementAtOrDefault(len + 2));
            }
        }

        static IEnumerable<(T, T?, T?)> Iterate(IEnumerable<T> source)
        {
            using var enumerator = source.GetEnumerator();

            while (enumerator.TryMoveNext(out var first)) {
                if (!enumerator.TryMoveNext(out var second)) {
                    yield return (first, default, default);
                    yield break;
                }

                if (!enumerator.TryMoveNext(out var third)) {
                    yield return (first, second, default);
                    yield break;
                }

                yield return (first, second, third);
            }
        }
    }
}
