﻿using Microsoft.CodeAnalysis;

namespace Trarizon.Library.Roslyn.Extensions;
public static class ProviderExtensions
{
    public static IncrementalValuesProvider<T> OfNotNull<T>(this IncrementalValuesProvider<T?> provider) where T : class
        => provider.Where(x => x is not null)!;
}
