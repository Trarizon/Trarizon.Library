using System.Numerics;

namespace Trarizon.Library.Collections.Extensions.Helpers;
internal static class QueryUtil
{
#if NET7_0_OR_GREATER

    public static bool IsInOrder<T>(T left, T right, bool descending) where T : IComparisonOperators<T, T, bool>
        // desc : prev >= curr
        // asc  : prev <= curr
        => descending && left >= right || left <= right;

#endif

    public static bool IsInOrder<T>(T left, T right, IComparer<T> comparer, bool descending)
        => comparer.Compare(left, right) is var res
        && descending && res >= 0 || res <= 0;

    public static T SelfSelector<T>(this T obj) => obj;
}
