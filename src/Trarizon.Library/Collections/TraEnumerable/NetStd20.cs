#if NETSTANDARD2_0

using System.Collections;

namespace Trarizon.Library.Collections;
partial class TraEnumerable
{
    public static bool TryGetNonEnumeratedCount<T>(this IEnumerable<T> source, out int count)
    {
        if (source is ICollection<T> collection) {
            count = collection.Count;
            return true;
        }
        if (source is ICollection ngcollection) {
            count = ngcollection.Count;
            return true;
        }
        count = 0;
        return false;
    }
}

#endif