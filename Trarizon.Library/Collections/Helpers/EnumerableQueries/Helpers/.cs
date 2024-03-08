using System.Diagnostics.CodeAnalysis;

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
}
