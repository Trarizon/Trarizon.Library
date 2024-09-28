namespace Trarizon.Library.Collections;
partial class TraEnumerable
{
    public static bool IsDistinct<T>(this IEnumerable<T> source, IEqualityComparer<T>? comparer = null)
    {
        HashSet<T> visited = new(comparer);
        foreach (var item in source) {
            if (!visited.Add(item))
                return false;
        }
        return true;
    }

    public static bool IsDistinctBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector, IEqualityComparer<TKey>? comparer = null)
    {
        HashSet<TKey> visited = new(comparer);
        foreach (var item in source) {
            if (!visited.Add(keySelector(item)))
                return false;
        }
        return true;
    }
}
