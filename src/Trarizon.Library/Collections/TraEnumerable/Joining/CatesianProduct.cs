namespace Trarizon.Library.Collections;
partial class TraEnumerable
{
    public static IEnumerable<(T, T2)> CartesianProduct<T, T2>(this IEnumerable<T> first, IEnumerable<T2> second)
    {
        if (first.IsEmptyArray() || second.IsEmptyArray())
            return [];

        return Iterate();

        IEnumerable<(T, T2)> Iterate()
        {
            foreach (var item1 in first) {
                foreach (var item2 in second) {
                    yield return (item1, item2);
                }
            }
        }
    }
}
