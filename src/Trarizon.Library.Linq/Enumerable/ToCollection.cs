using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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
