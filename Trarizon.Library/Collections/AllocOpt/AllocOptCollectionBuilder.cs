using Microsoft.VisualBasic;
using System.ComponentModel;

namespace Trarizon.Library.Collections.AllocOpt;
[EditorBrowsable(EditorBrowsableState.Never)]
public static class AllocOptCollectionBuilder
{
    #region Create

    public static AllocOptList<T> CreateList<T>(ReadOnlySpan<T> values)
    {
        var list = new AllocOptList<T>(values.Length);
        list.AddRange(values);
        return list;
    }

    public static AllocOptStack<T> CreateStack<T>(ReadOnlySpan<T> values)
    {
        var stack = new AllocOptStack<T>(values.Length);
        stack.PushRange(values);
        return stack;
    }

    public static AllocOptQueue<T> CreateQueue<T>(ReadOnlySpan<T> values)
    {
        var queue = new AllocOptQueue<T>(values.Length);
        queue.EnqueueRange(values);
        return queue;
    }

    public static AllocOptSet<T> CreateSet<T>(ReadOnlySpan<T> values)
    {
        var set = new AllocOptSet<T>(values.Length);
        foreach (var item in values) {
            set.Add(item);
        }
        return set;
    }

    public static AllocOptDictionary<TKey, TValue> CreateDictionary<TKey, TValue>(ReadOnlySpan<KeyValuePair<TKey, TValue>> values) where TKey : notnull
    {
        var dict = new AllocOptDictionary<TKey, TValue>(values.Length);
        foreach (var item in values) {
            dict.Add(item.Key, item.Value);
        }
        return dict;
    }

    #endregion

    #region As

    /// <summary>
    /// Create a list using <paramref name="array"/> as underlying array,
    /// initialized size is <paramref name="size"/>
    /// </summary>
    public static AllocOptList<T> AsList<T>(T[] array, int size)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(size, array.Length);
        return new(array, size);
    }

    /// <summary>
    /// Create a stack using <paramref name="array"/> as underlying array,
    /// initialized size is <paramref name="size"/>, and <c>array[^1]</c> is stack top
    /// </summary>
    public static AllocOptStack<T> AsStack<T>(T[] array, int size)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(size, array.Length);
        return new(array, size);
    }

    /// <summary>
    /// Create a queue using <paramref name="array"/> as underlying array,
    /// from <paramref name="headIndex"/>(contains) to <paramref name="tailIndex"/>
    /// </summary>
    /// <param name="headIndex">Index of the value to be dequeue</param>
    /// <param name="tailIndex">Index of the new enqueued value</param>
    public static AllocOptQueue<T> AsQueue<T>(T[] array, Index headIndex, Index tailIndex)
    {
        var head = headIndex.GetOffset(array.Length);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(head, array.Length, nameof(headIndex));
        var tail= tailIndex.GetOffset(array.Length);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(tail, array.Length, nameof(tailIndex));

        return new(array, headIndex.GetOffset(array.Length), tailIndex.GetOffset(array.Length));
    }

    #endregion
}
