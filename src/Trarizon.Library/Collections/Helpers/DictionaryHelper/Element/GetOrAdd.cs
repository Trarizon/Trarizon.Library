using System.Runtime.InteropServices;

namespace Trarizon.Library.Collections.Helpers;
partial class DictionaryHelper
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
    public static TValue GetOrAdd<TKey, TValue, TArgs>(this Dictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TArgs, TValue> factory, TArgs factoryArgs) where TKey : notnull
    {
        ref TValue? value = ref CollectionsMarshal.GetValueRefOrAddDefault(dictionary, key, out var exists);
        if (!exists)
            value = factory(key, factoryArgs);
        return value!;
    }

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
        value = factory(key);
        dictionary.Add(key, value);
        return value;
    }
    public static TValue GetOrAdd<TKey, TValue, TArgs>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TArgs, TValue> factory, TArgs factoryArgs) where TKey : notnull
    {
        if (dictionary.TryGetValue(key, out var value))
            return value;
        value = factory(key, factoryArgs);
        dictionary.Add(key, value);
        return value;
    }
}
