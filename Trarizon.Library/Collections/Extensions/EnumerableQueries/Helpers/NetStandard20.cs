#if NETSTANDARD2_0

namespace Trarizon.Library.Collections.Extensions;
partial class EnumerableQuery
{
    public static bool TryGetNonEnumeratedCount<T>(this IEnumerable<T> source, out int count)
    {
        if (source is ICollection<T> collection) {
            count = collection.Count;
            return true;
        }

        count = default;
        return false;
    }
}

#endif