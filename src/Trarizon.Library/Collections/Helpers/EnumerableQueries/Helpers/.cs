using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Trarizon.Library.Collections.Helpers;
[SuppressMessage("Style", "IDE0301")] // Use explicit on Empty<T>(), collection will alloc new List<T> in some conditions
public static partial class EnumerableQuery
{
    private static bool TryIterate<T>(this IEnumerator<T> enumerator, int count, out int iteratedCount)
    {
        iteratedCount = 0;
        if (count <= 0)
            return true;

        while (enumerator.MoveNext()) {
            if (++iteratedCount >= count)
                return true;
        }
        return false;
    }

    private static bool IsCheapEmpty<T>(this IEnumerable<T> source)
    {
        return source.TryGetNonEnumeratedCount(out var count) && count == 0;
    }
}
