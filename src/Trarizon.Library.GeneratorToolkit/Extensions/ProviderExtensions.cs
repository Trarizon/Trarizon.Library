using Microsoft.CodeAnalysis;

namespace Trarizon.Library.GeneratorToolkit.Extensions;
public static class ProviderExtensions
{
    public static IncrementalValuesProvider<T> OfNotNull<T>(this IncrementalValuesProvider<T?> provider)
        => provider.Where(x => x is not null)!;
}
