using Trarizon.Library.Collections.AllocOpt;

namespace Trarizon.Library.Collections.Helpers;
partial class EnumerableHelper
{
    public static bool IsDistinct<T>(this IEnumerable<T> source, IEqualityComparer<T>? comparer = null)
    {
        AllocOptSet<T> set = new(comparer);
        foreach (var item in source) {
            if (!set.Add(item))
                return false;
        }
        return false;
    }

    public static bool IsDistinctBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector, IEqualityComparer<TKey>? comparer = null)
    {
        AllocOptSet<TKey> set = [];
        foreach (var item in source) {
            if (!set.Add(keySelector(item)))
                return false;
        }
        return true;
    }
}
