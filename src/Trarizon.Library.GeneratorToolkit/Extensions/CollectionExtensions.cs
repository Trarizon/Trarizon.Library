using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

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

    public static StringBuilder AppendJoin(this StringBuilder builder,string join, IEnumerable<string> values)
    {
        using var enumerator = values.GetEnumerator();
        if(!enumerator.MoveNext())
            return builder;

        builder.Append(enumerator.Current);
        while (enumerator.MoveNext()) {
            builder.Append(join).Append(enumerator.Current);
        }
        return builder;
    }
}
