using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Trarizon.Library.Roslyn.Pipeline.Collections;

namespace Trarizon.Library.Roslyn.Pipeline;
public static partial class PipelineEqualityHelpers
{
    public static SequenceEquatableImmutableArray<T> ToSequenceEquatableImmutableArray<T>(this ImmutableArray<T> source)
        => new(source);

    public static SequenceEquatableImmutableArray<T> ToSequenceEquatableImmutableArray<T>(this IEnumerable<T> source)
        => new(source.ToImmutableArray());

    public static SequenceEquatableImmutableArray<T> ToSequenceEquatableImmutableArray<T>(this Span<T> source)
        => new(source.ToImmutableArray());

    public static SequenceEquatableImmutableArray<T> ToSequenceEquatableImmutableArray<T>(this ReadOnlySpan<T> source)
        => new(source.ToImmutableArray());
}
