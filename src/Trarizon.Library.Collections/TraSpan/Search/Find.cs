namespace Trarizon.Library.Collections;
public static partial class TraSpan
{
    public static int FindIndex<T, TArgs>(this ReadOnlySpan<T> span, TArgs args, Func<T, TArgs, bool> predicate)
    {
        for (int i = 0; i < span.Length; i++) {
            if (predicate(span[i], args))
                return i;
        }
        return -1;
    }

    public static int FindIndex<T>(this ReadOnlySpan<T> span, Func<T, bool> predicate)
    {
        for (int i = 0; i < span.Length; i++) {
            if (predicate(span[i]))
                return i;
        }
        return -1;
    }

    public static T? Find<T, TArgs>(this ReadOnlySpan<T> span, TArgs args, Func<T, TArgs, bool> predicate)
    {
        foreach (T item in span) {
            if (predicate(item, args))
                return item;
        }
        return default;
    }

    public static T? Find<T>(this ReadOnlySpan<T> span, Func<T, bool> predicate)
    {
        foreach (T item in span) {
            if (predicate(item))
                return item;
        }
        return default;
    }

    public static int FindLowerBoundIndex<T, TComparer>(this ReadOnlySpan<T> span, T key, TComparer comparer) where TComparer : IComparer<T>
        => FindLowerBoundIndex(span, new TraComparison.ComparerComparable<T, TComparer>(key, comparer));

    public static int FindLowerBoundIndex<T, TComparable>(this ReadOnlySpan<T> span, TComparable key) where TComparable : IComparable<T>
    {
        var index = span.BinarySearch(new TraComparison.GreaterOrNotComparable<T, TComparable>(key));
        return index < 0 ? ~index : index;
    }

    public static int FindUpperBoundIndex<T, TComparer>(this ReadOnlySpan<T> span, T key, TComparer comparer) where TComparer : IComparer<T>
        => FindUpperBoundIndex(span, new TraComparison.ComparerComparable<T, TComparer>(key, comparer));

    public static int FindUpperBoundIndex<T, TComparable>(this ReadOnlySpan<T> span, TComparable key) where TComparable : IComparable<T>
    {
        var index = span.BinarySearch(new TraComparison.LessOrNotComparable<T, TComparable>(key));
        return index < 0 ? ~index : index;
    }
}