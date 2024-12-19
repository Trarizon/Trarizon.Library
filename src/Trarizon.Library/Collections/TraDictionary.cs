using System.Runtime.InteropServices;

namespace Trarizon.Library.Collections;
public static class TraDictionary
{
#if !NETSTANDARD2_0

    public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue addValue) where TKey : notnull
    {
        ref TValue? value = ref CollectionsMarshal.GetValueRefOrAddDefault(dictionary, key, out var exists);
        if (!exists)
            value = addValue;
        return value!;
    }

    public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> factory) where TKey : notnull
    {
        ref TValue? value = ref CollectionsMarshal.GetValueRefOrAddDefault(dictionary, key, out var exists);
        if (!exists)
            value = factory(key);
        return value!;
    }

    public static TValue GetOrAdd<TKey, TValue, TArgs>(this Dictionary<TKey, TValue> dictionary, TKey key, TArgs factoryArgs, Func<TKey, TArgs, TValue> factory) where TKey : notnull
    {
        ref TValue? value = ref CollectionsMarshal.GetValueRefOrAddDefault(dictionary, key, out var exists);
        if (!exists)
            value = factory(key, factoryArgs);
        return value!;
    }

#endif

    public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue addValue) where TKey : notnull
    {
        if (dictionary.TryGetValue(key, out var value))
            return value;

        dictionary.Add(key, addValue);
        return addValue;
    }

    public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> factory) where TKey : notnull
    {
        if (dictionary.TryGetValue(key, out var value))
            return value;

        var addValue = factory(key);
        dictionary.Add(key, addValue);
        return addValue;
    }

    public static TValue GetOrAdd<TKey, TValue, TArgs>(this IDictionary<TKey, TValue> dictionary, TKey key, TArgs factoryArgs, Func<TKey, TArgs, TValue> factory) where TKey : notnull
    {
        if (dictionary.TryGetValue(key, out var value))
            return value;

        var addValue = factory(key, factoryArgs);
        dictionary.Add(key, addValue);
        return addValue;
    }
}
