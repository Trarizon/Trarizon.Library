using Trarizon.Library.Collections.Helpers;

namespace Trarizon.Library.Collections;

public static partial class TraCollection
{
    /// <summary>
    /// Get the item at <paramref name="index"/>, from the top
    /// </summary>
    public static T At<T>(this Stack<T> stack, int index)
        => UnsafeAccess<T>.GetUnderlyingArray(stack)[stack.Count - 1 - index];

    /// <summary>
    /// Get the item at <paramref name="index"/>, from the head
    /// </summary>
    public static T At<T>(this Queue<T> queue, int index)
    {
        var size = queue.Count;
        Throws.ThrowIfIndexGreaterThanOrEqual(index, size);

        var array = UnsafeAccess<T>.GetArray(queue);
        var head = UnsafeAccess<T>.GetHead(queue);
        var idx = head + index;
        if (idx < array.Length)
            return array[idx];

        idx -= array.Length;
        return array[idx];
    }
}
