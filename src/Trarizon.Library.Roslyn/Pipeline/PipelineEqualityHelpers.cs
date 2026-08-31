using System.Collections.Immutable;

namespace Trarizon.Library.Roslyn.Pipeline;

public static partial class PipelineEqualityHelpers
{
    public static EquatableImmutableArray<T> ToEquatableImmutableArray<T>(this ImmutableArray<T> source)
        => new(source);

    public static EquatableImmutableArray<T> ToEquatableImmutableArray<T>(this IEnumerable<T> source)
        => new(source.ToImmutableArray());

    public static EquatableImmutableArray<T> ToEquatableImmutableArray<T>(this Span<T> source)
        => new(source.ToImmutableArray());

    public static EquatableImmutableArray<T> ToEquatableImmutableArray<T>(this ReadOnlySpan<T> source)
        => new(source.ToImmutableArray());

    public static EquatableReadOnlyMemory<T> AsEquatableReadOnlyMemory<T>(this Memory<T> source)
        => new(source);

    public static EquatableReadOnlyMemory<T> AsEquatableReadOnlyMemory<T>(this ReadOnlyMemory<T> source)
        => new(source);

    public static EquatableReadOnlyMemory<T> AsEquatableReadOnlyMemory<T>(this ImmutableArray<T> source)
        => new(source.AsMemory());

    public static EquatableReadOnlyMemory<T> AsEquatableReadOnlyMemory<T>(this T[] source)
        => new(source.AsMemory());

    public static EquatableReadOnlyMemory<T> AsEquatableReadOnlyMemory<T>(this EquatableImmutableArray<T> source)
        => new(source.Array.AsMemory());
}
