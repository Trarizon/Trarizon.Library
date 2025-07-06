﻿namespace Trarizon.Library.Linq;
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

    /// <summary>
    /// Check if the size of collection is less than <paramref name="count"/>
    /// </summary>
    /// <param name="actualCount">
    /// If the size of collection is less than <paramref name="count"/>,this is the size of collection, 
    /// else, this equals to <paramref name="count"/>
    /// </param>
    public static bool CountsLessThan<T>(this IEnumerable<T> source, int count, out int actualCount) => !source.CountsAtLeast(count, out actualCount);

    /// <summary>
    /// Check if the size of collection is less than or equals to <paramref name="count"/>
    /// </summary>
    /// <param name="actualCount">
    /// If the size of collection is less than <paramref name="count"/>, this is the size of collection, 
    /// else, this equals to <paramref name="count"/>
    /// </param>
    public static bool CountsAtMost<T>(this IEnumerable<T> source, int count, out int actualCount) => !source.CountsMoreThan(count, out actualCount);

    /// <summary>
    /// Check if the size of collection is equal to <paramref name="count"/>
    /// </summary>
    public static bool CountsEqualsTo<T>(this IEnumerable<T> source, int count)
        => source.CountsAtMost(count, out int actualCount) && actualCount == count;

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
}
