using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Trarizon.Library.Collections.Extensions;
public static partial class DictionaryExtensions
{
    #region GetOrAdd

#if NET8_0_OR_GREATER

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

    #endregion

    #region AddOrUpdate

#if NET8_0_OR_GREATER

    public static void AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> addFactory, Func<TKey, TValue, TValue> updateFunc) where TKey : notnull
    {
        ref var val = ref CollectionsMarshal.GetValueRefOrNullRef(dictionary, key);
        if (Unsafe.IsNullRef(ref val))
            val = addFactory(key);
        else
            val = updateFunc(key, val);
    }
    public static void AddOrUpdate<TKey, TValue, TArgs>(this Dictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TArgs, TValue> addFactory, Func<TKey, TValue, TArgs, TValue> updateFunc, TArgs factoryArgs) where TKey : notnull
    {
        ref var val = ref CollectionsMarshal.GetValueRefOrNullRef(dictionary, key);
        if (Unsafe.IsNullRef(ref val))
            val = addFactory(key, factoryArgs);
        else
            val = updateFunc(key, val, factoryArgs);
    }

#endif

    #endregion
}
