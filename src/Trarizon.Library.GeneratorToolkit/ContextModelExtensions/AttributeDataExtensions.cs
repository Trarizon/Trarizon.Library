using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using Trarizon.Library.Collections;

namespace Trarizon.Library.GeneratorToolkit.ContextModelExtensions;
public static class AttributeDataExtensions
{
    public static Optional<T> GetNamedArgument<T>(this AttributeData attribute, string parameterName)
    {
        if (attribute.NamedArguments.TryFirst(kv => kv.Key == parameterName, out var first))
            return (T)first.Value.Value!;

        return default;
    }

    public static ImmutableArray<T> GetNamedArrayArgument<T>(this AttributeData attribute, string parameterName)
    {
        if (attribute.NamedArguments.TryFirst(kv => kv.Key == parameterName, out var first)) {
            var arr = first.Value.Values;
            if (arr is [])
                return [];
            return arr.Select(arg => (T)arg.Value!).ToImmutableArray();
        }
        return default;
    }

    public static Optional<T> GetConstructorArgument<T>(this AttributeData attribute, int index)
    {
        if (attribute.ConstructorArguments is var args && args.TryAt(index, out var val))
            return (T)val.Value!;

        return default;
    }

    public static ImmutableArray<T> GetConstructorArrayArgument<T>(this AttributeData attribute, int index)
    {
        if (attribute.ConstructorArguments is var args && args.TryAt(index, out var val)) {
            var arr = val.Values;
            if (arr is [])
                return [];
            return arr.Select(arg => (T)arg.Value!).ToImmutableArray();
        }
        else
            return default;
    }
}
