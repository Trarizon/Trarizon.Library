namespace Trarizon.Library.Collections.Helpers;
partial class EnumerableQuery
{
    // TODO: Optimize
    public static IEnumerable<(T, T2)> CartesianProduct<T, T2>(this IEnumerable<T> first, IEnumerable<T2> second)
    {
        foreach (var item in first) {
            foreach (var item2 in second) {
                yield return (item, item2);
            }
        }
    }
}
