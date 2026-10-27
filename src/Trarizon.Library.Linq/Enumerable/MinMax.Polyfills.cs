namespace Trarizon.Library.Linq;

partial class TraEnumerable
{
#if NETSTANDARD

    public static T Min<T>(this IEnumerable<T> source, IComparer<T> comparer)
    {
        var val = TryMin(source, new CmpOrder<T>(comparer), out var success);
        if (!success)
            Throws.CollectionIsEmpty();
        return val;
    }

    public static T Max<T>(this IEnumerable<T> source, IComparer<T> comparer)
    {
        var val = TryMin(source, new RCmpOrder<T>(comparer), out var success);
        if (!success)
            Throws.CollectionIsEmpty();
        return val;
    }

#endif
}
