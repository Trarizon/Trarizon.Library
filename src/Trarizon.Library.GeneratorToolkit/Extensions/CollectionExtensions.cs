using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Trarizon.Library.GeneratorToolkit.Extensions;
public static class CollectionExtensions
{
    public static Optional<T> TryAt<T>(this ImmutableArray<T> values, int index)
    {
        if (values.IsDefault || index < 0 || index >= values.Length)
            return default;
        else
            return values[index];
    }

    public static ImmutableArray<T> EmptyIfDefault<T>(this ImmutableArray<T> array)
        => array.IsDefault ? ImmutableArray<T>.Empty : array;
}
