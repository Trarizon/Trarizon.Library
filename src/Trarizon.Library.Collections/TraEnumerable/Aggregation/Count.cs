namespace Trarizon.Library.Collections;
public static partial class TraEnumerable
{
    /// <summary>
    /// Check if the size of collection is greater than <paramref name="count"/>
    /// </summary>
    /// <param name="actualCount">
    /// If the size of collection is less than <paramref name="count"/>,this is the size of collection, 
    /// else, this equals to <paramref name="count"/>
    /// </param>
    public static bool CountsMoreThan<T>(this IEnumerable<T> source, int count, out int actualCount)
    {
        if (count < 0) {
            actualCount = count;
            return true;
        }

        if (source.TryGetNonEnumeratedCount(out actualCount)) {
            if (actualCount > count) {
                actualCount = count;
                return true;
            }
            else {
                return false;
            }
        }

        using var enumerator = source.GetEnumerator();
        if (enumerator.TryIterate(count, out actualCount)) {
            return enumerator.MoveNext();
        }
        return false;
    }

    public static bool CountsMoreThan<T>(this IEnumerable<T> source, int count, Func<T, bool> predicate, out int actualCount)
    {
        if (count < 0) {
            actualCount = count;
            return true;
        }

        using var enumerator = source.GetEnumerator();
        actualCount = 0;
        while (enumerator.MoveNext()) {
            if (predicate(enumerator.Current)) {
                actualCount++;
                if (actualCount > count)
                    return true;
            }
        }
        return false;
    }

    public static bool CountsMoreThan<T>(this IEnumerable<T> source, int count, Func<T, bool>? predicate = null)
        => predicate is null
        ? source.CountsMoreThan(count, out _)
        : source.CountsMoreThan(count, predicate, out _);

    /// <summary>
    /// Check if the size of collection is greater than or equals to <paramref name="count"/>,
    /// </summary>
    /// <param name="actualCount">
    /// If the size of collection is less than <paramref name="count"/>, this is the size of collection, 
    /// else, this equals to <paramref name="count"/>
    /// </param>
    public static bool CountsAtLeast<T>(this IEnumerable<T> source, int count, out int actualCount)
    {
        if (count <= 0) {
            actualCount = count;
            return true;
        }

        if (source.TryGetNonEnumeratedCount(out actualCount)) {
            if (actualCount < count) {
                return false;
            }
            else {
                actualCount = count;
                return true;
            }
        }

        // enumCount = 0;
        using var enumerator = source.GetEnumerator();
        return enumerator.TryIterate(count, out actualCount);
    }

    public static bool CountsAtLeast<T>(this IEnumerable<T> source, int count, Func<T, bool> predicate, out int actualCount)
    {
        if (count <= 0) {
            actualCount = count;
            return true;
        }

        using var enumerator = source.GetEnumerator();
        actualCount = 0;
        while (enumerator.MoveNext()) {
            if (predicate(enumerator.Current)) {
                actualCount++;
                if (actualCount >= count)
                    return true;
            }
        }
        return false;
    }

    public static bool CountsAtLeast<T>(this IEnumerable<T> source, int count, Func<T, bool>? predicate = null)
        => predicate is null
        ? source.CountsAtLeast(count, out _)
        : source.CountsAtLeast(count, predicate, out _);

    /// <summary>
    /// Check if the size of collection is less than <paramref name="count"/>
    /// </summary>
    /// <param name="actualCount">
    /// If the size of collection is less than <paramref name="count"/>,this is the size of collection, 
    /// else, this equals to <paramref name="count"/>
    /// </param>
    public static bool CountsLessThan<T>(this IEnumerable<T> source, int count, out int actualCount) => !source.CountsAtLeast(count, out actualCount);

    public static bool CountsLessThan<T>(this IEnumerable<T> source, int count, Func<T, bool> predicate, out int actualCount) => !source.CountsAtLeast(count, predicate, out actualCount);

    public static bool CountsLessThan<T>(this IEnumerable<T> source, int count, Func<T, bool>? predicate = null)
        => predicate is null
        ? source.CountsLessThan(count, out _)
        : source.CountsLessThan(count, predicate, out _);

    /// <summary>
    /// Check if the size of collection is less than or equals to <paramref name="count"/>
    /// </summary>
    /// <param name="actualCount">
    /// If the size of collection is less than <paramref name="count"/>, this is the size of collection, 
    /// else, this equals to <paramref name="count"/>
    /// </param>
    public static bool CountsAtMost<T>(this IEnumerable<T> source, int count, out int actualCount) => !source.CountsMoreThan(count, out actualCount);

    public static bool CountsAtMost<T>(this IEnumerable<T> source, int count, Func<T, bool> predicate, out int actualCount) => !source.CountsMoreThan(count, predicate, out actualCount);

    public static bool CountsAtMost<T>(this IEnumerable<T> source, int count, Func<T, bool>? predicate = null)
        => predicate is null
        ? source.CountsAtMost(count, out _)
        : source.CountsAtMost(count, predicate, out _);

    public static bool CountsEqualsTo<T>(this IEnumerable<T> source, int count, Func<T, bool>? predicate = null)
        => predicate is null
        ? source.CountsAtMost(count, out int actualCount) && actualCount == count
        : source.CountsAtMost(count, predicate, out actualCount) && actualCount == count;

    /// <summary>
    /// Check if the size of collection is between <paramref name="min"/> and <paramref name="max"/>,
    /// false if <paramref name="max"/> is less than <paramref name="min"/>
    /// </summary>
    /// <param name="actualCount">
    /// If the size of collection is less than <paramref name="max"/>, this is the size of collection, 
    /// else, this equals to <paramref name="max"/>
    /// </param>
    public static bool CountsBetween<T>(this IEnumerable<T> source, int min, int max, out int actualCount)
    {
        if (max < min) {
            actualCount = 0;
            return false;
        }

        return source.CountsAtMost(max, out actualCount) && actualCount >= min;
    }

    public static bool CountsBetween<T>(this IEnumerable<T> source, int min, int max, Func<T, bool> predicate, out int actualCount)
    {
        if (max < min) {
            actualCount = 0;
            return false;
        }
        return source.CountsAtMost(max, predicate, out actualCount) && actualCount >= min;
    }

    public static bool CountsBetween<T>(this IEnumerable<T> source, int min, int max, Func<T, bool>? predicate = null)
        => predicate is null
        ? source.CountsBetween(min, max, out _)
        : source.CountsBetween(min, max, predicate, out _);
}
