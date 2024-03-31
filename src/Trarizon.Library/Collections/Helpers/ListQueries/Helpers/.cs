using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Trarizon.Library.Collections.Helpers;
[SuppressMessage("Style", "IDE0301")] // Use explicit on Empty<T>(), collection will alloc new List<T> in some conditions
public static partial class ListQuery
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ListWrapper<T> Wrap<T>(this IReadOnlyList<T> list) => Unsafe.As<IReadOnlyList<T>, ListWrapper<T>>(ref list);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsFixedSizeEmpty<T>(this IList<T> list) => list.Count == 0 && list.IsFixedSize();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsFixedSizeEmpty<T>(this IReadOnlyList<T> list) => list.Count == 0 && list.IsFixedSize();

    // Mirrored in PopFront
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsFixedSizeAtMost<T>(this IList<T> list, int maxSize) => list.Count <= maxSize && list.IsFixedSize();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsFixedSizeAtMost<T>(this IReadOnlyList<T> list, int maxSize) => list.Count <= maxSize && list.IsFixedSize();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsFixedSize<T>(this IList<T> list) => list.IsReadOnly || list is T[];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsFixedSize<T>(this IReadOnlyList<T> list) => list is T[];
}
