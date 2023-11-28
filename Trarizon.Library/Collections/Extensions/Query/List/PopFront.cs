namespace Trarizon.Library.Collections.Extensions.Query;
partial class ListQuery
{
    /// <summary>
    /// Pop specific number of elements, and return the rest,
    /// popped elements are cached in <paramref name="leadingElements"/>
    /// </summary>
    public static IList<T> PopFrontList<T>(this IList<T> list, int count, out IList<T> leadingElements)
    {
        if (count <= 0) {
            leadingElements = Array.Empty<T>();
            return list;
        }
        else if (count < list.Count) {
            leadingElements = list.TakeList(..count);
            return list.TakeList(count..);
        }
        else {
            leadingElements = list;
            return Array.Empty<T>();
        }
    }

    /// <summary>
    /// Pop first element and return the rest,
    /// first element is <paramref name="firstElement"/>.
    /// If no element in <paramref name="list"/>,
    /// <paramref name="firstElement"/> is <paramref name="defaultValue"/>
    /// </summary>
    public static IList<T> PopFirstList<T>(this IList<T> list, out T firstElement, T defaultValue = default!)
    {
        switch (list.Count) {
            case 0:
                firstElement = defaultValue;
                return list;
            case 1:
                firstElement = list[0];
                return Array.Empty<T>();
            default:
                firstElement = list[0];
                return list.TakeList(1);
        }
    }

    /// <summary>
    /// Pop elements until <paramref name="predicate"/> failed.
    /// popped elements are cached in <paramref name="leadingElements"/>
    /// </summary>
    public static IList<T> PopFrontWhileList<T>(this IList<T> list, out IList<T> leadingElements, Func<T, bool> predicate)
    {
        for (int i = 0; i < list.Count; i++) {
            if (!predicate(list[i])) {
                if (i == 0) {
                    leadingElements = Array.Empty<T>();
                    return list;
                }
                else {
                    leadingElements = list.TakeList(..i);
                    return list.TakeList(i);
                }
            }
        }
        leadingElements = list;
        return Array.Empty<T>();
    }
}
