using Trarizon.Library.Collections.AllocOpt;

namespace Trarizon.Library.Collections;
partial class TraEnumerable
{
    public static IEnumerable<(T, T2)> CartesianProduct<T, T2>(this IEnumerable<T> first, IEnumerable<T2> second)
    {
        if (first.IsEmptyArray() || second.IsEmptyArray())
            return [];

        return Iterate();

        IEnumerable<(T, T2)> Iterate()
        {
            using var enumerator = first.GetEnumerator();
            if (!enumerator.MoveNext())
                yield break;

            using var enumerator2 = second.GetEnumerator();

            if (!enumerator2.MoveNext())
                yield break;

            AllocOptList<T2> cache = [];
            var cur = enumerator.Current;
            var cur2 = enumerator2.Current;
            yield return (cur, cur2);
            cache.Add(cur2);
            while (enumerator2.MoveNext()) {
                cur2 = enumerator2.Current;
                yield return (cur, cur2);
                cache.Add(cur2);
            }

            while (enumerator.MoveNext()) {
                cur = enumerator.Current;
                foreach (var item in cache) {
                    yield return (cur, item);
                }
            }
        }
    }
}
