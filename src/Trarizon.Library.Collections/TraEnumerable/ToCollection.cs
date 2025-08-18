using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Trarizon.Library.Collections.AllocOpt;
using Trarizon.Library.Collections.Buffers;

namespace Trarizon.Library.Collections;
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

    public static IArrayAllocator<T>.AutoReleaseScope<IArrayAllocatorExt.ArrayPoolAllocator<T>> ToPooledArray<T>(this IEnumerable<T> source, out int count)
    {
        AllocOptList<T> builder = new();
        builder.AddRange(source);
        count = builder.Count;
        return builder.GetUnderlyingArray();
    }

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
