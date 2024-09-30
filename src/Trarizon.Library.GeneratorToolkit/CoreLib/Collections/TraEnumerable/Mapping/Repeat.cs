namespace Trarizon.Library.GeneratorToolkit.CoreLib.Collections;
partial class TraEnumerable
{
    public static IEnumerable<T> Repeat<T>(this IEnumerable<T> source, int count)
    {
        if (count == 1)
            return source;

        if (count <= 0 || source.IsEmptyArray())
            return [];

        if (source is IList<T> ilist) {
            if (source is T[] array)
                return IterateArray(array);
            else if (source is List<T> list)
                return IterateList(list);
            else
                return IterateIList(ilist);
        }

        return Iterate();

        IEnumerable<T> IterateArray(T[] array)
        {
            for (int i = 0; i < count; i++) {
                foreach (var item in array) {
                    yield return item;
                }
            }
        }

        IEnumerable<T> IterateList(List<T> list)
        {
            for (int i = 0; i < count; i++) {
                foreach (var item in list) {
                    yield return item;
                }
            }
        }

        IEnumerable<T> IterateIList(IList<T> list)
        {
            for (int i = 0; i < count; i++) {
                for (int j = 0; j < list.Count; j++) {
                    yield return list[j];
                }
            }
        }

        IEnumerable<T> Iterate()
        {
            if (source.TryGetNonEnumeratedCount(out var srcCount)) {
                T[] cache = new T[srcCount];
                int ic = 0;
                foreach (var item in source) {
                    cache[ic++] = item;
                    yield return item;
                }

                for (int i = 1; i < count; i++) {
                    foreach (var item in cache) {
                        yield return item;
                    }
                }
            }
            else {
                List<T> cache = new();
                foreach (var item in source) {
                    cache.Add(item);
                    yield return item;
                }

                for (int i = 1; i < count; i++) {
                    foreach (var item in cache) {
                        yield return item;
                    }
                }
            }
        }
    }
}
