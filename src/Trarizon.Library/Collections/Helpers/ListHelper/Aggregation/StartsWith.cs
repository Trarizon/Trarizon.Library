using Trarizon.Library.CodeAnalysis.MemberAccess;

namespace Trarizon.Library.Collections.Helpers;
partial class ListHelper
{
    [FriendAccess(typeof(EnumerableHelper))]
    internal static bool StartsWithList<T>(this IList<T> list, int offset, ReadOnlySpan<T> values)
        => StartsWithOpt(list, offset, values);

    public static bool StartsWithROList<T>(this IReadOnlyList<T> list, int offset, ReadOnlySpan<T> values)
        => StartsWithOpt(list.Wrap(), offset, values);


    [FriendAccess(typeof(EnumerableHelper))]
    internal static bool StartsWithList<T>(this IList<T> list, int offset, IEnumerable<T> values)
        => StartsWithOpt(list, offset, values);

    public static bool StartsWithROList<T>(this IReadOnlyList<T> list, int offset, IEnumerable<T> values)
        => StartsWithOpt(list.Wrap(), offset, values);


    private static bool StartsWithOpt<TList, T>(this TList list, int offset, ReadOnlySpan<T> values) where TList : IList<T>
    {
        if (offset + values.Length > list.Count)
            return false;

        for (int i = 0; i < values.Length; i++) {
            if (!EqualityComparer<T>.Default.Equals(list[i + offset], values[i]))
                return false;
        }

        return true;
    }

    private static bool StartsWithOpt<TList, T>(this TList list, int offset, IEnumerable<T> value) where TList : IList<T>
    {
        int count = list.Count;

        if (offset > count)
            return false;
        if (value.TryGetNonEnumeratedCount(out var valCount) && offset + valCount > count)
            return false;

        using var enumerator = value.GetEnumerator();
        for (int i = offset; i < count; i++) {
            if (!enumerator.MoveNext())
                return true;
            if (!EqualityComparer<T>.Default.Equals(list[i], enumerator.Current))
                return false;
        }

        return false;
    }
}
