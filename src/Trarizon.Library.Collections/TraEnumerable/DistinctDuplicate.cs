using System.Runtime.InteropServices;

namespace Trarizon.Library.Collections;
public static partial class TraEnumerable
{
    /// <summary>
    /// Check if sequence has no repeated item
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if sequence has no repeated item
    /// </returns>
    public static bool IsDistinct<T>(this IEnumerable<T> source, IEqualityComparer<T>? comparer = null)
    {
        HashSet<T> visited = new(comparer);
        foreach (var item in source) {
            if (!visited.Add(item))
                return false;
        }
        return true;
    }

    /// <inheritdoc cref="IsDistinct{T}(IEnumerable{T}, IEqualityComparer{T}?)"/>
    public static bool IsDistinctBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector, IEqualityComparer<TKey>? comparer = null)
    {
        HashSet<TKey> visited = new(comparer);
        foreach (var item in source) {
            if (!visited.Add(keySelector(item)))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Filter items that not distinct in collection, note that the result sequence may not has same order with original sequence
    /// </summary>
    public static IEnumerable<T> Duplicates<T>(this IEnumerable<T> source, IEqualityComparer<T>? comparer = null)
    => source.DuplicatesBy(x => x, comparer);

    /// <summary>
    /// Filter items that not distinct in collection, note that the result sequence may not has same order with original sequence
    /// </summary>
    public static IEnumerable<T> DuplicatesBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector, IEqualityComparer<TKey>? comparer = null)
    {
        if (source is T[] arr) {
            return IterateIfSmallArray(arr, keySelector, comparer) ?? Iterate(source, keySelector, comparer);
        }
        return Iterate(source, keySelector, comparer);

        static IEnumerable<T>? IterateIfSmallArray(T[] array, Func<T, TKey> keySelector, IEqualityComparer<TKey>? comparer)
        {
            switch (array) {
                case [] or [_]:
                    return [];

                case [var v0, var v1]:
                    return (comparer ?? EqualityComparer<TKey>.Default).Equals(keySelector(v0), keySelector(v1))
                        ? array : [];
                case [var v0, var v1, var v2]:
                    var k0 = keySelector(v0);
                    comparer ??= EqualityComparer<TKey>.Default;
                    if (comparer.Equals(k0, keySelector(v1))) {
                        if (comparer.Equals(k0, keySelector(v2)))
                            return array;
                        else
                            return new T[] { v0, v1 };
                    }
                    else if (comparer.Equals(k0, keySelector(v2)))
                        return new T[] { v0, v2 };
                    else
                        return [];
            }
            return null;
        }

        static IEnumerable<T> Iterate(IEnumerable<T> source, Func<T, TKey> keySelector, IEqualityComparer<TKey>? comparer)
        {
            Dictionary<ValueTuple<TKey>, (bool Duplicated, T Value)> dict = comparer is null ? new() : new(new ValueTupleWrapEqualityComparer<TKey>(comparer));

            foreach (var item in source) {
#if NETSTANDARD
                var key = new ValueTuple<TKey>(keySelector(item));
                if (!dict.TryGetValue(new(keySelector(item)), out var val)) {
                    dict.Add(key, (false, item));
                }
#else
                ref var val = ref CollectionsMarshal.GetValueRefOrAddDefault(dict, new(keySelector(item)), out var exists);
                if (!exists) {
                    val = (false, item);
                }
#endif
                else if (val.Duplicated) {
                    yield return item;
                }
                else {
                    val.Duplicated = true;
                    yield return val.Value;
                    yield return item;
                }
            }
        }
    }

    private sealed class ValueTupleWrapEqualityComparer<T>(IEqualityComparer<T> comparer) : IEqualityComparer<ValueTuple<T>>
    {
        public bool Equals(ValueTuple<T> x, ValueTuple<T> y) => comparer.Equals(x.Item1, y.Item1);
        public int GetHashCode(ValueTuple<T> obj) => obj.Item1 is null ? 0 : comparer.GetHashCode(obj.Item1);
    }
}
