using System.Collections.Immutable;

namespace Trarizon.Library.Roslyn;

internal static class Polyfills
{
#if !IMMUTABLE_MARSHAL

    public static ImmutableArray<T> ToImmutableArray<T>(this ReadOnlySpan<T> span)
    {
        var builder = ImmutableArray.CreateBuilder<T>(span.Length);
        foreach (var item in span)
            builder.Add(item);
        return builder.MoveToImmutable();
    }

#endif
}
