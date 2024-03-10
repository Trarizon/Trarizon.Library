using System.ComponentModel;

namespace Trarizon.Library.Collections.AllocOpt;
[EditorBrowsable(EditorBrowsableState.Never)]
public static class AllocOptCollectionBuilder
{
    public static AllocOptList<T> CreateList<T>(ReadOnlySpan<T> values)
    {
        var list = new AllocOptList<T>(values.Length);
        list.AddRange(values);
        return list;
    }

    public static AllocOptStack<T> CreateStack<T>(ReadOnlySpan<T> values)
    {
        var stack = new AllocOptStack<T>(values.Length);
        foreach (var item in values) {
            stack.Push(item);
        }
        return stack;
    }

    public static AllocOptQueue<T> CreateQueue<T>(ReadOnlySpan<T> values)
    {
        var queue = new AllocOptQueue<T>(values.Length);
        foreach (var item in values) {
            queue.Enqueue(item);
        }
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
}
