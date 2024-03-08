using System.Numerics;

namespace Trarizon.Library.Collections.Helpers.Utilities;
internal static class QueryUtil
{
    public static bool IsInOrder<T>(T left, T right, bool descending) where T : IComparisonOperators<T, T, bool>
        // desc : prev >= curr
        // asc  : prev <= curr
        => descending && left >= right || left <= right;

    public static bool IsInOrder<T>(T left, T right, IComparer<T> comparer, bool descending)
        => comparer.Compare(left, right) is var res
        && descending && res >= 0 || res <= 0;
}
