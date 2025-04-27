using System.Buffers;
using System.Runtime.CompilerServices;
using Trarizon.Library.Collections.AllocOpt;
using Trarizon.Library.Collections.Buffers;
#if NETSTANDARD2_0
using RuntimeHelpers = Trarizon.Library.Collections.Helpers.PfRuntimeHelpers;
#endif

namespace Trarizon.Library.Collections;
public static partial class TraEnumerable
{
    public static IEnumerable<T> Repeat<T>(this IEnumerable<T> source, int count)
    {
        if (count == 1)
            return source;

        if (count <= 0 || source.IsEmptyArray())
            return [];

        if (source is IList<T> ilist) {
            if (source is T[] array)
                return IterateArray(array, count);
            else if (source is List<T> list)
                return IterateList(list, count);
            else
                return IterateIList(ilist, count);
        }

        return Iterate(source, count);

        static IEnumerable<T> IterateArray(T[] array, int count)
        {
            for (int i = 0; i < count; i++) {
                foreach (var item in array) {
                    yield return item;
                }
            }
        }

        static IEnumerable<T> IterateList(List<T> list, int count)
        {
            using (ArrayPool<T>.Shared.Rent(list.Count, out var cache, RuntimeHelpers.IsReferenceOrContainsReferences<T>())) {
                int ci = 0;
                foreach (var item in list) {
                    cache[ci++] = item;
                    yield return item;
                }
                for (int i = 1; i < count; i++) {
                    foreach (var item in cache) {
                        yield return item;
                    }
                }
            }
        }

        static IEnumerable<T> IterateIList(IList<T> list, int count)
        {
            using (ArrayPool<T>.Shared.Rent(list.Count, out var cache, RuntimeHelpers.IsReferenceOrContainsReferences<T>())) {
                int ci = 0;
                for (int i = 0; i < list.Count; i++) {
                    T item = list[i];
                    cache[ci++] = item;
                    yield return item;
                }
                for (int i = 1; i < count; i++) {
                    foreach (var item in cache) {
                        yield return item;
                    }
                }
            }
        }

        static IEnumerable<T> Iterate(IEnumerable<T> source, int count)
        {
            if (source.TryGetNonEnumeratedCount(out var srcCount)) {
                using (ArrayPool<T>.Shared.Rent(srcCount, out var cache, RuntimeHelpers.IsReferenceOrContainsReferences<T>())) {
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
            }
            else {
                using var cache = new AllocOptList<T>();
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
