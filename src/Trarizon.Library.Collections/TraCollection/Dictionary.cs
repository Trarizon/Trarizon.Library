using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Trarizon.Library.Collections.Helpers;

namespace Trarizon.Library.Collections;
public static partial class TraCollection
{
#if NET8_0_OR_GREATER

    public static ref TValue AtRef<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key) where TKey : notnull
    {
        ref var value = ref CollectionsMarshal.GetValueRefOrNullRef(dictionary, key);
        if (Unsafe.IsNullRef(ref value))
            Throws.KeyNotFound(key, nameof(Dictionary<,>));
        return ref value!;
    }

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

    public static TValue AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue addValue, Func<TKey, TValue, TValue> updateFactory) where TKey : notnull
    {
        ref TValue? value = ref CollectionsMarshal.GetValueRefOrAddDefault(dictionary, key, out var exists);
        if (exists)
            value = updateFactory(key, value!);
        else
            value = addValue;
        return value;
    }

    public static TValue AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> addFactory, Func<TKey, TValue, TValue> updateFactory) where TKey : notnull
    {
        ref TValue? value = ref CollectionsMarshal.GetValueRefOrAddDefault(dictionary, key, out var exists);
        if (exists)
            value = updateFactory(key, value!);
        else
            value = addFactory(key);
        return value;
    }

    public static TValue AddOrUpdate<TKey, TValue, TArgs>(this Dictionary<TKey, TValue> dictionary, TKey key, TArgs factoryArgs, Func<TKey, TArgs, TValue> addFactory, Func<TKey, TArgs, TValue, TValue> updateFactory) where TKey : notnull
    {
        ref TValue? value = ref CollectionsMarshal.GetValueRefOrAddDefault(dictionary, key, out var exists);
        if (exists)
            value = updateFactory(key, factoryArgs, value!);
        else
            value = addFactory(key, factoryArgs);
        return value;
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

    public static TValue AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue addValue, Func<TKey, TValue, TValue> updateFactory) where TKey : notnull
    {
        if (dictionary.TryGetValue(key, out var value)) {
            var newValue = updateFactory(key, value);
            dictionary[key] = newValue;
            return newValue;
        }
        dictionary.Add(key, addValue);
        return addValue;
    }

    public static TValue AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> addFactory, Func<TKey, TValue, TValue> updateFactory) where TKey : notnull
    {
        if (dictionary.TryGetValue(key, out var value)) {
            var newValue = updateFactory(key, value);
            dictionary[key] = newValue;
            return newValue;
        }
        var addValue = addFactory(key);
        dictionary.Add(key, addValue);
        return addValue;
    }

    public static TValue AddOrUpdate<TKey, TValue, TArgs>(this IDictionary<TKey, TValue> dictionary, TKey key, TArgs factoryArgs, Func<TKey, TArgs, TValue> addFactory, Func<TKey, TArgs, TValue, TValue> updateFactory) where TKey : notnull
    {
        if (dictionary.TryGetValue(key, out var value)) {
            var newValue = updateFactory(key, factoryArgs, value);
            dictionary[key] = newValue;
            return newValue;
        }
        var addValue = addFactory(key, factoryArgs);
        dictionary.Add(key, addValue);
        return addValue;
    }
}
