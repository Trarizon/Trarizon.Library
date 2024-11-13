namespace Trarizon.Library;
public static class TraTuple
{
    public static KeyValuePair<TKey, TValue> ToKeyValuePair<TKey, TValue>(this (TKey Key, TValue Value) tuple)
        => KeyValuePair.Create(tuple.Key, tuple.Value);
}
