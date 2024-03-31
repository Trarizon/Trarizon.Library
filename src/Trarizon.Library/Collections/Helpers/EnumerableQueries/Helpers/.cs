using System.Collections;
using System.Runtime.CompilerServices;

namespace Trarizon.Library.Collections.Helpers;
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsFixedSizeEmpty<T>(this IEnumerable<T> source)
        => source is T[] { Length: 0 };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsFixedSizeEmpty(this IEnumerable source)
        => source is Array { Length: 0 };
}
