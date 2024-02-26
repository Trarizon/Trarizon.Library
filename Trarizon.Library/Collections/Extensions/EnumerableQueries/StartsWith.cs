namespace Trarizon.Library.Collections.Extensions;
partial class EnumerableQuery
{
    /// <summary>
    /// Determines whether the sequence after offsetting by <paramref name="offset"/> is start with <paramref name="value"/>
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="offset"/> is out of range
    /// </exception>
    public static bool StartsWith<T>(this IEnumerable<T> source, int offset, ReadOnlySpan<T> value)
    {
        if (source is IList<T> list) {
            return list.StartsWithList(offset, value);
        }

        using var enumerator = source.GetEnumerator();

        // source.Count < startIndex
        if (!enumerator.TryIterate(offset, out _))
            return false;

        int index = 0;
        while (enumerator.MoveNext()) {
            // all item in span iterated
            if (index >= value.Length)
                return true;

            if (!EqualityComparer<T>.Default.Equals(enumerator.Current, value[index]))
                return false;
            index++;
        }

        return false;
    }

    /// <summary>
    /// Determines whether the sequence after offsetting by <paramref name="offset"/> is start with <paramref name="value"/>
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="offset"/> is out of range
    /// </exception>
    public static bool StartsWith<T>(this IEnumerable<T> source, int offset, T[] value)
        => source.StartsWith(offset, value.AsSpan());

    /// <summary>
    /// Determines whether the sequence after offsetting by <paramref name="offset"/> is start with <paramref name="value"/>
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="offset"/> is out of range
    /// </exception>
    public static bool StartsWith<T>(this IEnumerable<T> source, int offset, IEnumerable<T> value)
    {
        if (source is IList<T> list) {
            return list.StartsWithList(offset, value);
        }

        using var enumerator = source.GetEnumerator();

        // source.Count < startIndex
        if (!enumerator.TryIterate(offset, out _))
            return false;

        using var valueEnumerator = value.GetEnumerator();
        while (enumerator.MoveNext()) {
            // all item in value iterated
            if (!valueEnumerator.MoveNext())
                return true;

            if (!EqualityComparer<T>.Default.Equals(enumerator.Current, valueEnumerator.Current))
                return false;
        }

        return false;
    }
}
