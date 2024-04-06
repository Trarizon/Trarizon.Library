using System.Numerics;

namespace Trarizon.Library.Collections.Helpers.Utilities;
internal static class Utility
{
    public static bool IsInAscOrder<T>(T former, T latter) where T : IComparisonOperators<T, T, bool>
        => former <= latter;

    public static bool IsInDescOrder<T>(T former, T latter) where T : IComparisonOperators<T, T, bool>
        => former >= latter;

    public static bool IsInAscOrder<T>(this IComparer<T> comparer, T former, T latter)
        => comparer.Compare(former, latter) <= 0;

    public static bool IsInDescOrder<T>(this IComparer<T> comparer, T former, T latter)
        => comparer.Compare(former, latter) >= 0;
}
