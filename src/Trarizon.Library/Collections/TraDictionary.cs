using System.Runtime.InteropServices;

namespace Trarizon.Library.Collections;
public static class TraDictionary
{
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
}
