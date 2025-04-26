using Trarizon.Library.Collections.AllocOpt;
using Trarizon.Library.Collections.Generic;
using Trarizon.Library.Collections.Specialized;

namespace Trarizon.Library.Collections;
public static class CollectionBuilders
{
    public static AllocOptList<T> CreateAllocOptList<T>(ReadOnlySpan<T> values)
    {
        var list = new AllocOptList<T>(values.Length);
        list.AddRange(values);
        return list;
    }

    public static ContiguousLinkedList<T> CreateContiguousLinkedList<T>(ReadOnlySpan<T> values)
    {
        var list = new ContiguousLinkedList<T>();
        list.EnsureCapacity(values.Length);
        foreach (var item in values) {
            list.AddLast(item);
        }
        return list;
    }

    public static Deque<T> CreateDeque<T>(ReadOnlySpan<T> values)
    {
        var queue = new Deque<T>();
        queue.EnsureCapacity(values.Length);
        queue.EnqueueRangeLast(values);
        return queue;
    }

    public static ListDictionary<TKey, TValue> CreateListDictionary<TKey, TValue>(ReadOnlySpan<KeyValuePair<TKey, TValue>> values) where TKey : notnull
    {
        var dict = new ListDictionary<TKey, TValue>();
        dict.EnsureCapacty(values.Length);
        foreach (var item in values) {
            dict.Add(item.Key, item.Value);
        }
        return dict;
    }
}
