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

        if (list.IsFixedSizeAtMost(count)) {
            leadingElements = list;
            return Array.Empty<T>();
        }

        leadingElements = list.TakeList(..count);
        return list.TakeList(count..);
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

        if (list.IsFixedSizeAtMost(count)) {
            leadingElements = list;
            return Array.Empty<T>();
        }

        leadingElements = list.TakeROList(..count);
        return list.TakeROList(count..);
    }


    public static IList<T> PopFrontList<T>(this IList<T> list, Span<T> resultSpan, out int resultLength)
    {
        if (resultSpan.Length == 0) {
            resultLength = 0;
            return list;
        }

        if (list is T[] arr) {
            if (arr.Length <= resultSpan.Length) {
                resultLength = arr.Length;
                arr.AsSpan().CopyTo(resultSpan);
                return Array.Empty<T>();
            }
            else {
                resultLength = resultSpan.Length;
                arr.AsSpan(0, resultLength).CopyTo(resultSpan);
                return list.TakeList(resultLength);
            }
        }

        if (list is List<T> lst) {
            if (lst.Count <= resultSpan.Length) {
                resultLength = lst.Count;
                CollectionsMarshal.AsSpan(lst).CopyTo(resultSpan);
                return Array.Empty<T>();
            }
            else {
                resultLength = resultSpan.Length;
                CollectionsMarshal.AsSpan(lst)[..resultLength].CopyTo(resultSpan);
                return list.TakeList(resultLength);
            }
        }

        if (list.IsReadOnly && list.Count is var len && len <= resultSpan.Length) {
            resultLength = len;
            for (int i = 0; i < resultLength; i++)
                resultSpan[i] = list[i];
            return Array.Empty<T>();
        }
        else {
            resultLength = resultSpan.Length;
            for (int i = 0; i < resultLength; i++)
                resultSpan[i] = list[i];
            return list.TakeList(resultLength);
        }
    }

    public static IReadOnlyList<T> PopFrontROList<T>(this IReadOnlyList<T> list, Span<T> resultSpan, out int resultLength)
    {
        if (resultSpan.Length == 0) {
            resultLength = 0;
            return list;
        }

        if (list is T[] arr) {
            if (arr.Length <= resultSpan.Length) {
                resultLength = arr.Length;
                arr.AsSpan().CopyTo(resultSpan);
                return Array.Empty<T>();
            }
            else {
                resultLength = resultSpan.Length;
                arr.AsSpan(0, resultLength).CopyTo(resultSpan);
                return list.TakeROList(resultLength);
            }
        }

        if (list is List<T> lst) {
            if (lst.Count <= resultSpan.Length) {
                resultLength = lst.Count;
                CollectionsMarshal.AsSpan(lst).CopyTo(resultSpan);
                return Array.Empty<T>();
            }
            else {
                resultLength = resultSpan.Length;
                CollectionsMarshal.AsSpan(lst)[..resultLength].CopyTo(resultSpan);
                return list.TakeROList(resultLength);
            }
        }

        if (list.Count is var len && len <= resultSpan.Length) {
            resultLength = len;
            for (int i = 0; i < resultLength; i++)
                resultSpan[i] = list[i];
            return Array.Empty<T>();
        }
        else {
            resultLength = resultSpan.Length;
            for (int i = 0; i < resultLength; i++)
                resultSpan[i] = list[i];
            return list.TakeROList(resultLength);
        }
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
            default:
                firstElement = list[0];
                return list.TakeROList(1);
        }
    }
}
