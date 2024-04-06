namespace Trarizon.Library.Collections.Helpers;
partial class EnumerableHelper
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
    /// Check if the size of collection is greater than <paramref name="count"/>
    /// </summary>
    public static bool CountsMoreThan<T>(this IEnumerable<T> source, int count) => source.CountsMoreThan(count, out _);

    /// <summary>
    /// Check if the size of collection is less than <paramref name="count"/>
    /// </summary>
    /// <param name="actualCount">
    /// If the size of collection is less than <paramref name="count"/>,this is the size of collection, 
    /// else, this equals to <paramref name="count"/>
    /// </param>
    public static bool CountsLessThan<T>(this IEnumerable<T> source, int count, out int actualCount)
    {
        if (count <= 0) {
            actualCount = count;
            return false;
        }

        if (source.TryGetNonEnumeratedCount(out actualCount)) {
            if (actualCount < count) {
                return true;
            }
            else {
                actualCount = count;
                return false;
            }
        }

        // enumCount = 0;
        using var enumerator = source.GetEnumerator();
        return !enumerator.TryIterate(count, out actualCount);
    }

    /// <summary>
    /// Check if the size of collection is less than <paramref name="count"/>
    /// </summary>
    public static bool CountsLessThan<T>(this IEnumerable<T> source, int count) => source.CountsLessThan(count, out _);

    /// <summary>
    /// Check if the size of collection is greater than or equals to <paramref name="count"/>,
    /// </summary>
    /// <param name="actualCount">
    /// If the size of collection is less than <paramref name="count"/>, this is the size of collection, 
    /// else, this equals to <paramref name="count"/>
    /// </param>
    public static bool CountsAtLeast<T>(this IEnumerable<T> source, int count, out int actualCount) => !source.CountsLessThan(count, out actualCount);

    /// <summary>
    /// Check if the size of collection is greater than or equals to <paramref name="count"/>,
    /// </summary>
    public static bool CountsAtLeast<T>(this IEnumerable<T> source, int count) => !source.CountsLessThan(count, out _);

    /// <summary>
    /// Check if the size of collection is less than or equals to <paramref name="count"/>
    /// </summary>
    /// <param name="actualCount">
    /// If the size of collection is less than <paramref name="count"/>, this is the size of collection, 
    /// else, this equals to <paramref name="count"/>
    /// </param>
    public static bool CountsAtMost<T>(this IEnumerable<T> source, int count, out int actualCount) => !source.CountsMoreThan(count, out actualCount);

    /// <summary>
    /// Check if the size of collection is less than or equals to <paramref name="count"/>
    /// </summary>
    public static bool CountsAtMost<T>(this IEnumerable<T> source, int count) => !source.CountsMoreThan(count, out _);

    /// <summary>
    /// Check if the size of collection is equals to <paramref name="count"/>
    /// </summary>
    public static bool CountsEqualsTo<T>(this IEnumerable<T> source, int count) => source.CountsAtMost(count, out int actualCount) && actualCount == count;

    /// <summary>
    /// Check if the size of collection is between <paramref name="lowerBound"/> and <paramref name="upperBound"/>,
    /// false if <paramref name="upperBound"/> is less than <paramref name="lowerBound"/>
    /// </summary>
    /// <param name="actualCount">
    /// If the size of collection is less than <paramref name="upperBound"/>, this is the size of collection, 
    /// else, this equals to <paramref name="upperBound"/>
    /// </param>
    public static bool CountsBetween<T>(this IEnumerable<T> source, int lowerBound, int upperBound, out int actualCount)
    {
        if (upperBound < lowerBound) {
            actualCount = 0;
            return false;
        }

        return source.CountsAtMost(upperBound, out actualCount) && actualCount >= lowerBound;
    }

    /// <summary>
    /// Check if the size of collection is between <paramref name="lowerBound"/> and <paramref name="upperBound"/>
    /// </summary>
    public static bool CountsBetween<T>(this IEnumerable<T> source, int lowerBound, int upperBound) => source.CountsBetween(lowerBound, upperBound, out _);
}
