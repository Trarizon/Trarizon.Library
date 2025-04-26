using System.Collections.Immutable;
using System.Runtime.InteropServices;

namespace Trarizon.Library.Collections;
public static partial class TraArray
{
    public static ImmutableArray<T> EmptyIfDefault<T>(this ImmutableArray<T> array)
        => array.IsDefault ? [] : array;

    /// <summary>
    /// Returns the underlying array as <see cref="IEnumerable{T}"/>, to get a better performance with linq.
    /// Note that the result maybe <see langword="null"/> if <paramref name="array"/> is default.
    /// </summary>
    /// <returns>
    /// A <see cref="IEnumerable{T}"/> representing the array, <see cref="null"/> if <paramref name="array"/> is default.
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

}
