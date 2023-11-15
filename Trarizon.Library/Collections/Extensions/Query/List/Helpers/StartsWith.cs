namespace Trarizon.Library.Collections.Extensions.Query;
partial class ListQuery
{
    // Public use EnumerableQuery.StartsWith()
    internal static bool StartsWithList<T>(this IList<T> list, int offset, ReadOnlySpan<T> value)
    {
        if (offset + value.Length > list.Count)
            return false;

        for (int i = 0; i < value.Length; i++) {
            if (!EqualityComparer<T>.Default.Equals(list[i + offset], value[i]))
                return false;
        }
        return true;
    }

    internal static bool StartsWithList<T>(this IList<T> list, int offset, IEnumerable<T> value)
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
