using Trarizon.Library.Collections.Comparisons;

namespace Trarizon.Library.Memory;

public static partial class TraSpan
{
    public static bool Contains<T, TEquatable>(this ReadOnlySpan<T> span, TEquatable value) where TEquatable : IEquatable<T>
    {
        foreach (var item in span)
        {
            if (value.Equals(item))
                return true;
        }
        return false;
    }

    public static int FindIndex<T, TArgs>(this ReadOnlySpan<T> span, TArgs args, Func<T, TArgs, bool> predicate)
    {
        for (int i = 0; i < span.Length; i++)
        {
            if (predicate(span[i], args))
                return i;
        }
        return -1;
    }

    public static int FindIndex<T>(this ReadOnlySpan<T> span, Func<T, bool> predicate)
    {
        for (int i = 0; i < span.Length; i++)
        {
            if (predicate(span[i]))
                return i;
        }
        return -1;
    }

    public static T? Find<T, TArgs>(this ReadOnlySpan<T> span, TArgs args, Func<T, TArgs, bool> predicate)
    {
        foreach (T item in span)
        {
            if (predicate(item, args))
                return item;
        }
        return default;
    }

    public static T? Find<T>(this ReadOnlySpan<T> span, Func<T, bool> predicate)
    {
        foreach (T item in span)
        {
            if (predicate(item))
                return item;
        }
        return default;
    }

    public static int FindLowerBoundIndex<T, TComparer>(this ReadOnlySpan<T> span, T key, TComparer comparer) where TComparer : IComparer<T>
        => FindLowerBoundIndex(span, new ComparerComparable<T, TComparer>(key, comparer));

    public static int FindLowerBoundIndex<T, TComparable>(this ReadOnlySpan<T> span, TComparable key) where TComparable : IComparable<T>
    {
        var index = span.BinarySearch(new GreaterOrNotComparable<T, TComparable>(key));
        return index < 0 ? ~index : index;
    }

    public static int FindUpperBoundIndex<T, TComparer>(this ReadOnlySpan<T> span, T key, TComparer comparer) where TComparer : IComparer<T>
        => FindUpperBoundIndex(span, new ComparerComparable<T, TComparer>(key, comparer));

    public static int FindUpperBoundIndex<T, TComparable>(this ReadOnlySpan<T> span, TComparable key) where TComparable : IComparable<T>
    {
        var index = span.BinarySearch(new LessOrNotComparable<T, TComparable>(key));
        return index < 0 ? ~index : index;
    }
}