using Trarizon.Library.CodeGeneration;

namespace Trarizon.Library.Collections.Generic;
public interface IKeyedEqualityComparer<T, TKey>
{
    bool Equals(T val, TKey key);

    int GetHashCode(TKey key);
}

public static class KeyedEqualityComparer<T, TKey>
{
    public static IKeyedEqualityComparer<T, TKey> Create(Func<T, TKey, bool> equals, Func<TKey, int> getHashCode)
        => new DelegateByKeyEqualityComparer<T, TKey>(equals, getHashCode);
}

[Singleton(InstancePropertyName = "Default", Options = SingletonOptions.NoProvider)]
internal sealed partial class PairByKeyEqualityComparer<TKey, TValue>
    : IKeyedEqualityComparer<(TKey, TValue), TKey>
    , IKeyedEqualityComparer<KeyValuePair<TKey, TValue>, TKey>
{
    public bool Equals((TKey, TValue) val, TKey key) => EqualityComparer<TKey>.Default.Equals(val.Item1, key);
    public bool Equals(KeyValuePair<TKey, TValue> val, TKey key) => EqualityComparer<TKey>.Default.Equals(val.Key, key);
    public int GetHashCode(TKey key) => key is null ? 0 : EqualityComparer<TKey>.Default.GetHashCode(key);
}

internal sealed class DelegateByKeyEqualityComparer<T, TKey>(Func<T, TKey, bool> equals, Func<TKey, int> getHashCode)
    : IKeyedEqualityComparer<T, TKey>
{
    public bool Equals(T val, TKey key) => equals(val, key);
    public int GetHashCode(TKey key) => getHashCode(key);
}