using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using Trarizon.Library.Roslyn.Collections.Comparisons;

namespace Trarizon.Library.Roslyn;

public static class ValueProviderExtensions
{
    public static IncrementalValueProvider<ImmutableArray<T>> WithImmutableArraySequenceComparer<T>(this IncrementalValueProvider<ImmutableArray<T>> provider)
        => provider.WithComparer(ImmutableArraySequenceEqualityComparer<T>.Default);

    public static IncrementalValuesProvider<ImmutableArray<T>> WithImmutableArraySequenceComparer<T>(this IncrementalValuesProvider<ImmutableArray<T>> provider)
        => provider.WithComparer(ImmutableArraySequenceEqualityComparer<T>.Default);

    public static IncrementalValuesProvider<T> OfNotNull<T>(this IncrementalValuesProvider<T?> provider) where T : class
        => provider.Where(x => x is not null)!;

    public static IncrementalValuesProvider<T> OfNotNull<T>(this IncrementalValuesProvider<T?> provider) where T : struct
        => provider.Where(x => x is not null).Select((x, _) => x!.Value);
}
