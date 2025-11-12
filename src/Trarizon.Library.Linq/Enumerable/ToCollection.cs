using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Collections.Immutable;

namespace Trarizon.Library.Linq;

public static partial class TraEnumerable
{
    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (var item in source) {
            action.Invoke(item);
        }
    }
 
    /// <summary>
    /// Returns the underlying array as <see cref="IEnumerable{T}"/>, to get a better performance with linq.
    /// Note that the result maybe <see langword="null"/> if <paramref name="array"/> is default.
    /// </summary>
    /// <returns>
    /// A <see cref="IEnumerable{T}"/> representing the array, <see langword="null"/> if <paramref name="array"/> is default.
    /// </returns>
    public static IEnumerable<T>? AsEnumerableOrNull<T>(this ImmutableArray<T> array)
        => ImmutableCollectionsMarshal.AsArray(array)!;

    /// <summary>
    /// Returns the underlying array as <see cref="IEnumerable{T}"/>, to get a better performance with linq.
    /// </summary>
    /// <returns>
    /// A <see cref="IEnumerable{T}"/> representing the array, if <paramref name="array"/> is default, returns empty array
    /// </returns>
    public static IEnumerable<T> AsEnumerable<T>(this ImmutableArray<T> array)
        => array.IsDefault ? [] : array.AsEnumerableOrNull()!;

    /// <summary>
    /// If <paramref name="source"/> is <see langword="null"/>, this method returns an empty collection,
    /// else return <paramref name="source"/> it self
    /// </summary>
    public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T>? source)
        => source ?? [];

    public static T[] ToPooledArray<T>(this IEnumerable<T> source, ArrayPool<T> pool, out int length)
    {
        if (source.TryGetNonEnumeratedCount(out length)) {
            var array = pool.Rent(length);
            int i = 0;
            foreach (var item in source) {
                array[i++] = item;
            }
            return array;
        }
        else {
            var tmp = source.ToArray();
            var array = pool.Rent(tmp.Length);
            return array;
        }
    }

    public static T[] ToPooledArray<T>(this IEnumerable<T> source,out int length)
        => source.ToPooledArray(ArrayPool<T>.Shared, out length);

    internal static bool TryGetSpan<T>(this IEnumerable<T> source, out ReadOnlySpan<T> span)
    {
        if (source.GetType() == typeof(T[])) {
            span = Unsafe.As<T[]>(source).AsSpan();
            return true;
        }
        if (source.GetType() == typeof(List<T>)) {
            var list = Unsafe.As<List<T>>(source);
#if NET8_0_OR_GREATER
            span = CollectionsMarshal.AsSpan(list);
#else
            span = Unsafe.As<StrongBox<T[]>>(list).Value;
#endif
            return true;
        }
        span = default;
        return false;
    }
}
