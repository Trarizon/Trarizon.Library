using System.Runtime.InteropServices;

namespace Trarizon.Library.Collections.Helpers;
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
    /// Pop specific number of elements, and return the rest,
    /// popped elements are cached in <paramref name="leadingElements"/>
    /// </summary>
    public static IReadOnlyList<T> PopFrontROList<T>(this IReadOnlyList<T> list, int count, out IReadOnlyList<T> leadingElements)
    {
        if (count <= 0) {
            leadingElements = Array.Empty<T>();
            return list;
        }
        else if (count < list.Count) {
            leadingElements = list.TakeROList(..count);
            return list.TakeROList(count..);
        }
        else {
            leadingElements = list;
            return Array.Empty<T>();
        }
    }


    public static IList<T> PopFrontList<T>(this IList<T> list, Span<T> resultSpan, out int resultLength)
    {
        if (resultSpan.Length == 0) {
            resultLength = 0;
            return list;
        }
        IList<T> result;
        if (resultSpan.Length < list.Count) {
            resultLength = resultSpan.Length;
            result = list.TakeList(resultLength..);
        }
        else {
            resultLength = resultSpan.Length;
            result = Array.Empty<T>();
        }

        if (list is T[] arr)
            arr.AsSpan(0, resultLength).CopyTo(resultSpan);
        else if (list is List<T> lst)
            CollectionsMarshal.AsSpan(lst).CopyTo(resultSpan);
        else {
            for (int i = 0; i < resultLength; i++)
                resultSpan[i] = list[i];
        }

        return result;
    }

    public static IReadOnlyList<T> PopFrontROList<T>(this IReadOnlyList<T> list, Span<T> resultSpan, out int resultLength)
    {
        if (resultSpan.Length == 0) {
            resultLength = 0;
            return list;
        }
        IReadOnlyList<T> result;
        if (resultSpan.Length < list.Count) {
            resultLength = resultSpan.Length;
            result = list.TakeROList(resultLength..);
        }
        else {
            resultLength = resultSpan.Length;
            result = Array.Empty<T>();
        }

        if (list is T[] arr)
            arr.AsSpan(0, resultLength).CopyTo(resultSpan);
        else if (list is List<T> lst)
            CollectionsMarshal.AsSpan(lst).CopyTo(resultSpan);
        else {
            for (int i = 0; i < resultLength; i++)
                resultSpan[i] = list[i];
        }

        return result;
    }


    /// <summary>
    /// Pop first element and return the rest,
    /// first element is <paramref name="firstElement"/>.
    /// If no element in <paramref name="list"/>,
    /// <paramref name="firstElement"/> is <paramref name="defaultValue"/>
    /// </summary>
    public static IList<T> PopFirstList<T>(this IList<T> list, out T? firstElement, T? defaultValue = default!)
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
    /// Pop first element and return the rest,
    /// first element is <paramref name="firstElement"/>.
    /// If no element in <paramref name="list"/>,
    /// <paramref name="firstElement"/> is <paramref name="defaultValue"/>
    /// </summary>
    public static IReadOnlyList<T> PopFirstROList<T>(this IReadOnlyList<T> list, out T? firstElement, T? defaultValue = default!)
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
                return list.TakeROList(1);
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

    /// <summary>
    /// Pop elements until <paramref name="predicate"/> failed.
    /// popped elements are cached in <paramref name="leadingElements"/>
    /// </summary>
    public static IReadOnlyList<T> PopFrontWhileROList<T>(this IReadOnlyList<T> list, out IReadOnlyList<T> leadingElements, Func<T, bool> predicate)
    {
        for (int i = 0; i < list.Count; i++) {
            if (!predicate(list[i])) {
                if (i == 0) {
                    leadingElements = Array.Empty<T>();
                    return list;
                }
                else {
                    leadingElements = list.TakeROList(..i);
                    return list.TakeROList(i);
                }
            }
        }
        leadingElements = list;
        return Array.Empty<T>();
    }
}
