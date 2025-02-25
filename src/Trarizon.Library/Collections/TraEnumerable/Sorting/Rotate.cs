using Trarizon.Library.Collections.AllocOpt;

namespace Trarizon.Library.Collections;
public static partial class TraEnumerable
{
    public static IEnumerable<T> Rotate<T>(this IEnumerable<T> source, int splitPosition)
    {
        if (splitPosition == 0)
            return source;

        if (source.TryGetNonEnumeratedCount(out var count) && splitPosition >= count)
            return source;

        return Iterate(source, splitPosition);

        static IEnumerable<T> Iterate(IEnumerable<T> source, int splitPosition)
        {
            AllocOptList<T> firstPart = new();

            using var enumerator = source.GetEnumerator();
            for (int i = 0; i < splitPosition; i++) {
                if (enumerator.MoveNext()) {
                    firstPart.Add(enumerator.Current);
                }
                else {
                    goto YieldFirstPart;
                }
            }

            while (enumerator.MoveNext()) {
                yield return enumerator.Current;
            }

        YieldFirstPart:
            foreach (var item in firstPart)
                yield return item;
        }
    }
}
