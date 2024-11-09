﻿using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Trarizon.Library.Collections;
public static partial class TraArray
{
    public static void MoveTo<T>(this T[] array, int fromIndex, int toIndex)
        => array.AsSpan().MoveTo(fromIndex, toIndex);

    public static void MoveTo<T>(this T[] array, Index fromIndex, Index toIndex)
        => array.AsSpan().MoveTo(fromIndex.GetOffset(array.Length), toIndex.GetOffset(array.Length));

    public static void MoveTo<T>(this T[] array, int fromIndex, int toIndex, int length)
        => array.AsSpan().MoveTo(fromIndex, toIndex, length);

    public static void MoveTo<T>(this T[] array, Range fromRange, Index toIndex)
        => array.AsSpan().MoveTo(fromRange, toIndex);

    public static ImmutableArray<T> EmptyIfDefault<T>(this ImmutableArray<T> array)
        => array.IsDefault ? [] : array;

    public static bool TryAt<T>(this ImmutableArray<T> array, int index, [MaybeNullWhen(false)] out T element)
    {
        if (array.IsDefault || index < 0 || index >= array.Length) {
            element = default!;
            return false;
        }
        element = array[index];
        return true;
    }

    /// <summary>
    /// Returns the underlying array as <see cref="IEnumerable{T}"/>, to get a better performance with linq.
    /// Note that the result maybe <see langword="null"/> if <paramref name="array"/> is default.
    /// </summary>
    /// <returns>
    /// A <see cref="IEnumerable{T}"/> representing the array, <see cref="null"/> if <paramref name="array"/> is default.
    /// </returns>
    public static IEnumerable<T>? AsEnumerable<T>(this ImmutableArray<T> array)
        => ImmutableCollectionsMarshal.AsArray(array)!;

    /// <summary>
    /// Returns the underlying array as <see cref="IEnumerable{T}"/>, to get a better performance with linq.
    /// </summary>
    /// <returns>
    /// A <see cref="IEnumerable{T}"/> representing the array, if <paramref name="array"/> is default, returns empty array
    /// </returns>
    public static IEnumerable<T> AsNotNullEnumerable<T>(this ImmutableArray<T> array)
        => array.IsDefault ? [] : array.AsEnumerable()!;
}
